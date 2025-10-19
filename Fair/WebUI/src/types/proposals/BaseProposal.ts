import { ProposalOption } from "./ProposalOption"

export type BaseProposal = {
  id: string

  title: string
  text: string

  creationTime: number

  options: ProposalOption[]

  optionVotesCount: number[]
  neitherCount: number
  anyCount: number
  banCount: number
  banishCount: number
}
