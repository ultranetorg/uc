import { PublicationImageBase } from "types"

import { BaseProposal } from "./BaseProposal"

export type ReviewProposal = {
  publication: PublicationImageBase
} & BaseProposal
