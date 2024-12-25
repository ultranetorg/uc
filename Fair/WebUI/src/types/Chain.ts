import { BaseRate } from "./BaseRate"

type ChainCost = {
  rentBytePerDay: bigint
  exeunit: bigint
  rentAccount: bigint
  rentAuthor: bigint[][]
  rentResource: bigint[]
  rentResourceForever: bigint
  rentResourceData: bigint[]
  rentResourceDataForever: bigint
}

type ChainState = {
  lastRoundId: number
  emission: number
  emissionInPct: number
  day: Date
  baseSize: number
  minimalMembershipBail: bigint
}

type ChainStatistics = {
  rounds: number
  transactions: number
  operations: number
  members: number
  authorsNormal: number
  authorsAuctioned: number
  resources: number
  analysesResults: number
}

type ChainPerformance = {
  operationsPerSecond: number
  transactionsPerSecond: number
}

export type Chain = {
  cost: ChainCost
  state: ChainState
  statistics: ChainStatistics
  performance: ChainPerformance
} & BaseRate
