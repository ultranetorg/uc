import { BaseFairOperation } from "types/fairOperations"
import { TransactionApe } from "types/node"

export type NodeApi = {
  outgoingTransaction(baseUrl: string, tag: string): Promise<TransactionApe>
  ping(baseUrl: string): Promise<boolean>
  transact(baseUrl: string, operations: BaseFairOperation[], userName: string): Promise<TransactionApe>
}
