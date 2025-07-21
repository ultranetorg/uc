import { BaseVotableOperation } from "./votableOperations"

export type AuthorReferendum = {
  id: string
  byId: string
  byNickname?: string
  byAddress: string
  byAvatar?: string
  yesCount: number
  noCount: number
  absCount: number
  expiration: number
  text: string
  option: BaseVotableOperation
}
