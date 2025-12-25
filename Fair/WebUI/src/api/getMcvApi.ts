import { BaseFairOperation } from "types"
import { TransactionApe } from "types/mcv"

import { McvApi } from "./McvApi"
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

const transact = async (baseUrl: string, operations: BaseFairOperation[], signer: string): Promise<TransactionApe> => {
  const mapped = operations.map(x => keysToPascalCase(x))

  const response = await fetch(`${baseUrl}/Transact`, {
    method: "POST",
    body: JSON.stringify({
      Operations: mapped,
      Signer: signer,
    }),
  })
  const data = await response.json()
  return keysToCamelCase(data) as TransactionApe
}

const api: McvApi = {
  outgoingTransaction,
  transact,
}

export const getMcvApi = () => api
