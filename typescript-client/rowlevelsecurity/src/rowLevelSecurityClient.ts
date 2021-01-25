/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
import { KoraliumRowLevelSecurityClient as GrpcClient } from './generated/rowlevelsecurity_grpc_pb'
import * as grpc from 'grpc';
import { RowLevelSecurityRequest, SqlOptions } from './generated/rowlevelsecurity_pb';

interface GetSqlFilterRequest {
  tableName: string
  tableAlias?: string
  headers?: { [name: string]: string }
}

export class KoraliumRowLevelSecurityClient {
  
  private client: GrpcClient

  constructor(url: string) {
    this.client = new GrpcClient(url, grpc.credentials.createInsecure())
  }

  getSqlFilter(tableName: string): Promise<string>;
  getSqlFilter(tableName: string, tableAlias: string): Promise<string>;
  getSqlFilter(tableName: string, headers: { [name: string]: string }): Promise<string>;
  getSqlFilter(tableName: string, tableAlias: string, headers: { [name: string]: string }): Promise<string>;

  getSqlFilter(tableName: string, arg1?: string | { [name: string]: string }, arg2?: { [name: string]: string }): Promise<string> {

    const parameters: GetSqlFilterRequest = {tableName: tableName}

    if (arg1) {
      if (typeof arg1 === 'string') {
        parameters.tableAlias = arg1
      } else {
        parameters.headers = arg1
      }
    }

    if (arg2) {
      parameters.headers = arg2
    }

    return this.getSqlFilter_internal(parameters)
  }

  private getSqlFilter_internal({ tableName, tableAlias, headers }: GetSqlFilterRequest): Promise<string> {
    const request = new RowLevelSecurityRequest();
    request.setTablename(tableName);
    request.setFormat(0);
    if (tableAlias) {
      const sqlOptions = new SqlOptions();
      sqlOptions.setTablealias(tableAlias);
      request.setSqloptions(sqlOptions);
    }
    
    const metadata = new grpc.Metadata();

    if (headers) {
      for (let [key, value] of Object.entries(headers)) {
        metadata.add(key, value as any);
      }
    }
    
    return new Promise<string>((resolve: any, reject: any) => {
      try {
        this.client.getRowLevelSecurityFilter(request, metadata, (error, data) => {
          if (error) {
            reject(error.message)
            return;
          }
          if (data) {
            resolve(data.getFilter())
          } else {
            reject("got invalid response.")
          }
        })
      }
      catch(error) {
        reject(error)
      }
      
    });
  }
}
