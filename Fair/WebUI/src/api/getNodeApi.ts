import { VAULT } from "constants/"
import { BaseFairOperation } from "types"
import { Pong, TransactionApe } from "types/node"

import { NodeApi } from "./NodeApi"
import { keysToCamelCase, keysToPascalCase } from "./utils"

const outgoingTransaction = async (baseUrl: string, tag: string): Promise<TransactionApe> => {
  const response = await fetch(`${baseUrl}/OutgoingTransaction`, {
    method: "POST",
    body: JSON.stringify({
      Tag: tag,
    }),
  })
  const data = await response.json()
  return keysToCamelCase(data) as TransactionApe
}

const ping = async (baseUrl: string): Promise<boolean> => {
  try {
    const res = await fetch(`${baseUrl}/Ping`)
    const data = await res.json()
    const normalized = keysToCamelCase(data) as Pong
    return normalized.status == "OK"
  } catch {
    return false
  }
}

const transact = async (
  baseUrl: string,
  operations: BaseFairOperation[],
  userName: string,
  session: string,
  signerAddress: string,
): Promise<TransactionApe> => {
  const mapped = operations.map(x => keysToPascalCase(x))

  const response = await fetch(`${baseUrl}/Transact`, {
    method: "POST",
    body: JSON.stringify({
      Application: VAULT.APPLICATION,
      Operations: mapped,
      User: userName,
      Session: session,
      Signer: signerAddress,
    }),
  })
  const data = await response.json()
  return keysToCamelCase(data) as TransactionApe
}

const api: NodeApi = {
  outgoingTransaction,
  ping,
  transact,
}

export const getNodeApi = () => api
