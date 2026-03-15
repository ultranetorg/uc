import { AccountBase } from "types"

import { BaseProposal } from "./BaseProposal"

export type ModeratorProposal = {
  moderators: AccountBase[] | undefined
} & BaseProposal
