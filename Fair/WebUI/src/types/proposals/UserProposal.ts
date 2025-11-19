import { AccountBaseAvatar } from "types"

import { BaseProposal } from "./BaseProposal"

export type UserProposal = {
  signer: AccountBaseAvatar
} & BaseProposal
