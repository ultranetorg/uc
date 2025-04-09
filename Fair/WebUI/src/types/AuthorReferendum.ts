import { BaseVotableOperation } from "./VotableOperations"

export type AuthorReferendum = {
  id: string
  yesCount: number
  noCount: number
  absCount: number
  expiration: number
  text: string
  proposal: BaseVotableOperation
}
