import { BaseRate } from "./BaseRate"
import { ChildItemsArray } from "./Collections"
import { ResourceInfo } from "./Resource"

export type Author = {
  name: string
  owner: string
  expirationDay: Date
  comOwner?: string
  orgOwner?: string
  netOwner?: string
  firstBidDay: Date
  lastWinner?: string
  lastBid: bigint
  lastBidDay: Date
  spaceReserved: number
  spaceUsed: number

  resources: ChildItemsArray<ResourceInfo>
} & BaseRate
