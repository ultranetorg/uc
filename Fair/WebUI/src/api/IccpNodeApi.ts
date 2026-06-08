import { BaseFairOperation } from "types"

import { TransactionApe } from "types/iccpNode"

export type IccpNodeApi = {
  outgoingTransaction(baseUrl: string, tag: string): Promise<TransactionApe>
  ping(baseUrl: string): Promise<boolean>
  transact(
    baseUrl: string,
    operations: BaseFairOperation[],
    userName: string,
    session: string,
    signerAddress: string,
  ): Promise<TransactionApe>
}
