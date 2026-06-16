import { BaseFairOperation } from "types"

import { TransactionApe } from "types/iccpNode"

export type IccpNodeApi = {
  outgoingTransaction(iccpNodeUrl: string, tag: string): Promise<TransactionApe>
  ping(iccpNodeUrl: string): Promise<boolean>
  transact(
    iccpNodeUrl: string,
    operations: BaseFairOperation[],
    userName: string,
    session: string,
    signerAddress: string,
  ): Promise<TransactionApe>
}
