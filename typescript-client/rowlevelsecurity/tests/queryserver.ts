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
import { GenericContainer, StartedTestContainer, Wait } from "testcontainers"
import path from "path"

export class QueryServer {

  container?: StartedTestContainer;

  async start() {
    console.log('starting')
     this.container = await new GenericContainer("koraliumwebtest")
      .withExposedPorts(5015)
      .withWaitStrategy(Wait.forLogMessage('Application started. Press Ctrl+C to shut down.'))
      .start();
  }

  async stop() {
    if(this.container != undefined) {
      await this.container.stop();
    }
  }

  getIpAddress() : string {
    if(this.container !== undefined) {
      return this.container.getContainerIpAddress();
    }
    throw new Error("Container is not initialized");
  }

  getPort() : number {    
    if(this.container !== undefined) {
      return this.container.getMappedPort(5016);
    }
    throw new Error("Container is not initialized");
  }
}