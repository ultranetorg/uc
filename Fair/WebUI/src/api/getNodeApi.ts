import { Pong } from "types/node/Pong"

import { NodeApi } from "./NodeApi"
import { keysToCamelCase } from "./utils"

const ping = async (): Promise<boolean> => {
  try {
    const res = await fetch("http://127.1.0.0:2900/node/v0/fair/Ping")
    const data = await res.json()
    const normalized = keysToCamelCase(data) as Pong
    return normalized.status == "OK"
  } catch {
    return false
  }
}

const api: NodeApi = {
  ping,
}

export const getNodeApi = () => api
