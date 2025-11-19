import { AccountBaseAvatar, BaseVotableOperation } from "types"

export type ProposalOption = {
  title: string
  operation: BaseVotableOperation
  yesAccounts: AccountBaseAvatar[]
}
