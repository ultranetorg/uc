import { PublicationImageBase } from "types"

import { Proposal } from "./Proposal"

export type PublicationProposal = {
  updationTime: number
  publication: PublicationImageBase
  authorId: string
  authorTitle: string
  authorLogoId?: string
} & Proposal
