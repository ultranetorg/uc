import { AccountBase } from "./AccountBase"

export type Proposal = {
  id: string

  title: string
  text: string

  byAccount: AccountBase

  creationTime: number
  expirationTime: number

  optionsVotesCount: number[]
  neitherCount: number
  absCount: number
  banCount: number
  banishCount: number

  commentsCount: number
}
