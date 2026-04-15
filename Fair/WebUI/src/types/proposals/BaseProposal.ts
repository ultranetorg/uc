import { AccountBase } from "types"
import { OperationType } from "types/OperationType"

import { ProposalOption } from "./ProposalOption"

export type BaseProposal = {
  id: string

  operation: OperationType

  optionsVotesCount: number[]
  neitherCount: number
  anyCount: number
  banCount: number
  banishCount: number

  creationTime: number

  title: string
  text: string

  options: ProposalOption[]

  by: AccountBase
  multipleOptions: boolean

  hoursLeft: number
}
