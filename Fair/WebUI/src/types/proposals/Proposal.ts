import { AccountBase } from "types"
import { OperationType } from "types/OperationType"

import { ProposalOption } from "./ProposalOption"

export type Proposal = {
  id: string

  operation: OperationType

  yes: string[][]
  neither: string[]
  any: string[]
  ban: string[]
  banish: string[]

  creationTime: number

  title: string
  text: string

  options: ProposalOption[]

  by: AccountBase
  multipleOptions: boolean

  hoursLeft: number
}
