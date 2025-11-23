import { AccountBaseAvatar, PublicationImageBase } from "types"
import { BaseProposal } from "./BaseProposal"

export type ReviewProposal = {
  reviewer: AccountBaseAvatar
  publication: PublicationImageBase
} & BaseProposal
