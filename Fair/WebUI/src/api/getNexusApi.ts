import { VAULT } from "constants/"
import { IccpNode } from "types/nexus"

import { NexusApi } from "./NexusApi"
import { keysToCamelCase } from "./utils"

const getIccpNodeUrl = async (nexusUrl: string): Promise<IccpNode> => {
  const response = await fetch(`${nexusUrl}/IccpNode`, {
    method: "POST",
    body: JSON.stringify({
      Net: VAULT.NETWORK,
    }),
  })
  const data = await response.json()
  return keysToCamelCase(data) as IccpNode
}

const api: NexusApi = {
  getIccpNodeUrl,
}

export const getNexusApi = () => api
