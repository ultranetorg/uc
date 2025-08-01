import { AccountBase, ProposalOption, PublicationImageBase } from "types"

export type ReviewProposal = {
  id: string
  creationTime: number
  options: ProposalOption[]
  reviewer: AccountBase
  publication: PublicationImageBase
}
