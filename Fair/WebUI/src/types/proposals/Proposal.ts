import { AccountBase } from "types"

import { BaseProposal } from "./BaseProposal"

export type Proposal = {
  byAccount: AccountBase
  commentsCount: number
} & BaseProposal
