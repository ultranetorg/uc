import { Proposal } from "./Proposal"
import { ProposalOption } from "./ProposalOption"

export type ProposalDetails = {
  options: ProposalOption[]
} & Proposal
