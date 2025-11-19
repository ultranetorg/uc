import { AccountBaseAvatar } from "types"

import { BaseProposal } from "./BaseProposal"

export type Proposal = {
  byAccount: AccountBaseAvatar
  commentsCount: number
} & BaseProposal
