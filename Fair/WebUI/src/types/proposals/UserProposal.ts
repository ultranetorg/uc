import { AccountBase } from "types"

import { BaseProposal } from "./BaseProposal"

export type UserProposal = {
  signer: AccountBase
} & BaseProposal
