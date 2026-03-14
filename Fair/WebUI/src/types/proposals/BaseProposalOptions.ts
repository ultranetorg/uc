import { BaseProposal } from "./BaseProposal"
import { ProposalOption } from "./ProposalOption"

export type BaseProposalOptions = {
  options: ProposalOption[]
} & BaseProposal
