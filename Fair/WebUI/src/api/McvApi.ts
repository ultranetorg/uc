import { BaseFairOperation } from "types/fairOperations"
import { TransactionApe } from "types/mcv"

export type McvApi = {
  outgoingTransaction(baseUrl: string, tag: string): Promise<TransactionApe>
  transact(baseUrl: string, operations: BaseFairOperation[], signer: string): Promise<TransactionApe>
}
