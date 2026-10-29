import { Creator, OperationType } from "types"

import { ProposalOption } from "./ProposalOption"

export type Proposal = {
  id: string

  isStalled: boolean

  operation: OperationType

  optionTitles?: string[]
  yes: string[][]
  neither: string[]
  any: string[]
  ban: string[]
  banish: string[]

  creationTime: number

  title: string
  text: string

  options: ProposalOption[]

  creator: Creator
  multipleOptions: boolean
}
