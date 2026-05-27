import { AccountBase } from "types"

import { Proposal } from "./Proposal"

export type PublisherProposal = {
  authors: AccountBase[] | undefined
} & Proposal
