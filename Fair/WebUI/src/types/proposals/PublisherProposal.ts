import { AccountBase } from "types"

import { BaseProposal } from "./BaseProposal"

export type PublisherProposal = {
  authors: AccountBase[] | undefined
} & BaseProposal
