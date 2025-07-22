import { BaseVotableOperation } from "./votableOperations"

export type ModeratorDiscussion = {
  id: string
  yesCount: number
  noCount: number
  absCount: number
  expiration: number
  text: string
  option: BaseVotableOperation
  commentsCount: number
}
