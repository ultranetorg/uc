import { NnpNode } from "types/nexus"

export type NexusApi = {
  getNodeUrl(baseUrl: string): Promise<NnpNode>
}
