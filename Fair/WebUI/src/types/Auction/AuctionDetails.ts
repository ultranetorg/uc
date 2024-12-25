import { BaseRate } from "types/BaseRate"
import { ChildItemsArray } from "types/Collections"

import { Auction } from "./Auction"

export type AuctionDetails = {
  lastWinner: string
  highestBidBy: string
  highestBid: number

  bids: ChildItemsArray<AuctionBid>
} & Auction &
  BaseRate

export type AuctionBid = {
  bid: number
  bidBy: string
  bidDay: Date

  name: string
}
