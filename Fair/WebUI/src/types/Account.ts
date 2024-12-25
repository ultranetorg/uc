import { BaseRate } from "./BaseRate"
import { ChildItemsArray } from "./Collections"
import { Operation } from "./Operations"

export type Account = {
  address: string
  balance: bigint
  // bail: bigint
  lastTransactionNid: number
  lastEmissionId: number
  // candidacyDeclarationRid: number
  averageUptime: bigint

  authors: AccountAuthor[]
  operations: ChildItemsArray<Operation>
} & BaseRate

export type AccountAuthor = {
  name: string
  expirationDay: Date
}
