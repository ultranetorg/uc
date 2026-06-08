import { AuthorBaseAvatar } from "types"

import { Proposal } from "./Proposal"

export type PublisherProposal = {
  authors: AuthorBaseAvatar[]
} & Proposal
