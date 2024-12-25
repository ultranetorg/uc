import { BaseRate } from "types/BaseRate"
import { ChildItemsArray } from "types/Collections"

import { RoundTransaction } from "./RoundTransaction"
import { RoundMember } from "./RoundMember"
import { RoundEmission } from "./RoundEmission"
import { RoundDomainBid } from "./RoundDomainBid"

export type Round = {
  id: number
  parentId: number
  hash: string
  consensusTime: Date
  consensusExeunitFee: bigint
  rentPerByte: bigint
  emission: bigint
  members: RoundMember[]
  funds?: string[]
  emissions?: RoundEmission[]
  domainBids?: RoundDomainBid[]
  feePerExecutionUnit: bigint
  membersCount: number
  consensusMemberLeaversCount: number
  consensusFundJoinersCount: number
  consensusFundLeaversCount: number
  consensusViolatorsCount: number

  transactions: ChildItemsArray<RoundTransaction>
} & BaseRate
