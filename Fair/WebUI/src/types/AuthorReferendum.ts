import { BaseVotableOperation } from "./votableOperations"

export type AuthorReferendum = {
  id: string
  yesCount: number
  noCount: number
  absCount: number
  expiration: number
  text: string
  proposal: BaseVotableOperation
}
