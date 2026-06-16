import { IccpNode } from "types/nexus"

export type NexusApi = {
  getIccpNodeUrl(nexusUrl: string): Promise<IccpNode>
}
