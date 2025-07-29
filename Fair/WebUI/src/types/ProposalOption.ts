import { AccountBase } from "./AccountBase"
import { BaseVotableOperation } from "./votableOperations"

export type ProposalOption = {
  title: string
  operation: BaseVotableOperation
  yesAccounts: AccountBase[]
}
