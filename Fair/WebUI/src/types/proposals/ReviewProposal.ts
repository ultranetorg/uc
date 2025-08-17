import { AccountBase, PublicationImageBase } from "types"
import { BaseProposal } from "./BaseProposal"

export type ReviewProposal = {
  reviewer: AccountBase
  publication: PublicationImageBase
} & BaseProposal
