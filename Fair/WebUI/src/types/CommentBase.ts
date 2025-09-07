import { AccountBase } from "./AccountBase"

export type CommentBase = {
  id: string
  creatorAccount: AccountBase
  text: string
  created: number
  rating?: number
}
