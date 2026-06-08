import { User } from "types"

import { Proposal } from "./Proposal"

export type ModeratorProposal = {
  moderators: User[]
} & Proposal
