import { AccountBase } from "types"

import { BaseProposal } from "./BaseProposal"

export type PublisherProposal = {
  additions: AccountBase[] | undefined
  removals: AccountBase[] | undefined
} & BaseProposal
