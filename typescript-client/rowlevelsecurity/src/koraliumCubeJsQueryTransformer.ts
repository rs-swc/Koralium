import { BinaryFilter, Query, QueryFilter } from "./cubejsModels";
import { KoraliumRowLevelSecurityClient } from "./rowLevelSecurityClient";

interface ServiceContainer {
  client: KoraliumRowLevelSecurityClient;
  tableName: string;
  cubeName: string;
}

export class KoraliumCubeJsQueryTransformer {

  private services: Map<string, ServiceContainer>;
  
  constructor(){
    this.services = new Map<string, ServiceContainer>();
  }

  addCubeTable(cubeName: string, koraliumTableName: string, url: string) {
    cubeName = cubeName.toLowerCase()
    this.services.set(cubeName, {
      client: new KoraliumRowLevelSecurityClient(url),
      cubeName: cubeName,
      tableName: koraliumTableName
    });
  }

  private getCubeNames(query: Query): Array<string> {
    const cubes: Array<string> = []
    query.dimensions?.forEach(x => {
      const splitDimension = x.split('.')
      if (splitDimension.length > 1) {
        cubes.push(splitDimension[0].toLowerCase())
      }
    })
    query.measures.forEach(x => {
      const splitMeasure = x.split('.')
      if (splitMeasure.length > 1) {
        cubes.push(splitMeasure[0].toLowerCase())
      }
    })

    query.timeDimensions?.forEach(x => {
      const splitDimension = x.dimension.split('.')
      if (splitDimension.length > 1) {
        cubes.push(splitDimension[0].toLowerCase())
      }
    })

    // Filter the list so it only contains cubes that actually requires filters
    return cubes.filter(x => {
      return this.services.get(x) !== undefined
    })
  }

  
  private async getCubeFilters(cubeName: string, token?: string): Promise<QueryFilter | BinaryFilter> {
    const service = this.services.get(cubeName)

    //Send query here to the 
    const filters: QueryFilter | BinaryFilter = {}

    if (service) {
      const headers: { [key: string]: string} = {};
      
      if (token) {
        headers['Authorization'] = `Bearer ${token}`
      }
      const filters = await service.client.getCubeJsFilter(service.tableName, service.cubeName, headers)
      return filters;
    }
    else {
      throw new Error("Internal client error, service was undefined");
    }
  }

  async transformQuery(query: Query, accessToken?: string): Promise<Query> {
    // Get all the cube names that is part of the query
    const cubeNames = this.getCubeNames(query)

    const filters = await Promise.all(cubeNames.map(x => this.getCubeFilters(x, accessToken)));
    
    if (query.filters === undefined) {
      query.filters = [];
    }
    
    for (let index = 0; index < filters.length; index++) {
      query.filters.push(filters[index]);
    }
    
    return query;
  }
}