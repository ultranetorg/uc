import { BaseRate } from "types"

import { OperationType } from "./OperationType"

export type Operation = {
  $type: OperationType
  id: string
  signer: string
  transactionId: string
  description: string
} & BaseRate
