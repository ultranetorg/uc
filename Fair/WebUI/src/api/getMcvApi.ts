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

const transact = async (baseUrl: string, operation: BaseFairOperation, signer: string): Promise<TransactionApe> => {
  const obj = keysToPascalCase(operation)

  const response = await fetch(`${baseUrl}/Transact`, {
    method: "POST",
    body: JSON.stringify({
      Operations: [obj],
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
