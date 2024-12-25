import { BaseRate } from "./BaseRate"
import { ChildItemsArray } from "./Collections"
import { Operation } from "./Operations"

export type Transaction = {
  id: string
  signer: string
  nid: number
  fee: bigint
  tag?: string
  roundId: number
  operationsCount: number

  operations: ChildItemsArray<Operation>
} & BaseRate
