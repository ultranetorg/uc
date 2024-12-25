import { Operation } from "./Operation"

export type UntTransfer = {
  to: string
  amount: bigint
} & Operation
