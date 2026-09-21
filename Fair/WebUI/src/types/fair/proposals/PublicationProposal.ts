import { PublicationImageBase } from "types"

import { Proposal } from "./Proposal"

export type PublicationProposal = {
  updated: number
  publication: PublicationImageBase
  authorId: string
  authorTitle: string
  authorLogoId?: string
} & Proposal
