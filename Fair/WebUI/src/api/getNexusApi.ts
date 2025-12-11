import { VAULT } from "constants/"
import { NnpNode } from "types/nexus"

import { NexusApi } from "./NexusApi"
import { keysToCamelCase } from "./utils"

const getFairUrl = async (baseUrl: string): Promise<NnpNode> => {
  const response = await fetch(`${baseUrl}/NnpNode`, {
    method: "POST",
    body: JSON.stringify({
      Net: VAULT.NETWORK,
    }),
  })
  const data = await response.json()
  return keysToCamelCase(data) as NnpNode
}

const api: NexusApi = {
  getFairUrl,
}

export const getNexusApi = () => api
