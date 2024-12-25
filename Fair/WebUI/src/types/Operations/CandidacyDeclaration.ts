import { Operation } from "./Operation"

export type CandidacyDeclaration = {
  bail: bigint
  baseRdcIPs: string[]
  seedHubRdcIPs: string[]
} & Operation
