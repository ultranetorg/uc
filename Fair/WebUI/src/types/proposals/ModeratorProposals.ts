import { AccountBase } from "types"

import { Proposal } from "./Proposal"

export type ModeratorProposal = {
  moderators: AccountBase[] | undefined
} & Proposal
