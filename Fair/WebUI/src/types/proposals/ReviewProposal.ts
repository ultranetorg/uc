import { PublicationImageBase } from "types"

import { Proposal } from "./Proposal"

export type ReviewProposal = {
  publication: PublicationImageBase | null
  reviewText: string
} & Proposal
