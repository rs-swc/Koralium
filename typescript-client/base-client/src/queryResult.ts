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
export class Metadata {
  customMetadata: {};

  constructor(customMetadata: {}) {
    this.customMetadata = customMetadata;
  }
}

export interface ResultArray<T> extends Iterable<T> {
  length: number
  get(index: number): T;
  toArray: () => Array<T>
}

export class QueryResult<T extends { [key: string]: any; } = any> {
  rows: ResultArray<T>;
  metadata: Metadata;

  constructor(rows: ResultArray<T>, metadata: Metadata) {
    this.rows = rows;
    this.metadata = metadata;
  }
}