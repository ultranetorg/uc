import { IccpNode } from "types/nexus"

export type NexusApi = {
  getIccpUrl(nexusUrl: string): Promise<IccpNode>
}
