import { NnpNode } from "types/nexus"

export type NexusApi = {
  getFairUrl(baseUrl: string): Promise<NnpNode>
}
