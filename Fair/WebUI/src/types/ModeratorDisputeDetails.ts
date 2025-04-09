import { ModeratorDispute } from "./ModeratorDispute"

export type ModeratorDisputeDetails = {
  pros: string[]
  cons: string[]
  abs: string[]
} & ModeratorDispute
