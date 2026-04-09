import { PublicationImageBase } from "types"

import { BaseProposal } from "./BaseProposal"

export type PublicationProposal = {
  updationTime: number
  publication: PublicationImageBase
  authorId: string
  authorTitle: string
  authorLogoId?: string
} & BaseProposal
