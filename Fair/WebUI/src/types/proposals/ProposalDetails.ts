import { BaseProposalOptions } from "./BaseProposalOptions"

export type ProposalDetails = {
  votesRequiredToWin: number
  neither: string[]
  any: string[]
  ban: string[]
  banish: string[]
} & BaseProposalOptions
