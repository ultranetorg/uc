import { BaseVotableOperation } from "./VotableOperations"

export type ModeratorDispute = {
  id: string
  yesCount: number
  noCount: number
  absCount: number
  expiration: number
  text: string
  proposal: BaseVotableOperation
}
