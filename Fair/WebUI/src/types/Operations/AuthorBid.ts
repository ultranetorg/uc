import { Operation } from "./Operation"

export type AuthorBid = {
  author: string
  bid: bigint
  tld: string
} & Operation
