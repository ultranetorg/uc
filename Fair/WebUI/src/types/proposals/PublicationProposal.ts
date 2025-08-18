import { AccountBase, PublicationImageBase } from "types"

import { BaseProposal } from "./BaseProposal"

export type PublicationProposal = {
  updationTime: number
  publication: PublicationImageBase
  author: AccountBase
} & BaseProposal
