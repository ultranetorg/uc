import { ProposalOption } from "types"

export type UserProposal = {
  id: string
  creationTime: number
  options: ProposalOption[]
}
