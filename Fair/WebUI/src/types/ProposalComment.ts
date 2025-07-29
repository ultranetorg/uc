import { AccountBase } from "./AccountBase"

export type ProposalComment = {
  id: string
  proposalId: string
  creatorAccount: AccountBase
  text: string
  created: number
}
