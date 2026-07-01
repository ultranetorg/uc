import { BaseFairOperation } from "types"
import { Pong, TransactionApe } from "types/iccpNode"

import { IccpNodeApi } from "./IccpNodeApi"
import { keysToCamelCase, keysToPascalCase } from "./utils"

const { VITE_APP_ICCP_NODE_TEST_URL: BASE_URL } = import.meta.env

const PING_TIMEOUT = 10000

const outgoingTransaction = async (iccpNodeUrl: string, tag: string): Promise<TransactionApe> => {
  const response = await fetch(`${iccpNodeUrl}/OutgoingTransaction`, {
    method: "POST",
    body: JSON.stringify({
      Tag: tag,
    }),
  })
  const data = await response.json()
  return keysToCamelCase(data) as TransactionApe
}

const ping = async (iccpNodeUrl: string): Promise<boolean> => {
  try {
    const res = await fetch(`${BASE_URL ?? iccpNodeUrl}/Ping`, { signal: AbortSignal.timeout(PING_TIMEOUT) })
    const data = await res.json()
    const normalized = keysToCamelCase(data) as Pong
    return normalized.status == "OK"
  } catch {
    return false
  }
}

const transact = async (
  iccpNodeUrl: string,
  operations: BaseFairOperation[],
  userName: string,
  session: string,
): Promise<TransactionApe> => {
  const mapped = operations.map(x => keysToPascalCase(x))

  const response = await fetch(`${iccpNodeUrl}/Transact`, {
    method: "POST",
    body: JSON.stringify({
      Operations: mapped,
      User: userName,
      Session: session,
    }),
  })
  const data = await response.json()
  return keysToCamelCase(data) as TransactionApe
}

const api: IccpNodeApi = {
  outgoingTransaction,
  ping,
  transact,
}

export const getIccpNodeApi = () => api
