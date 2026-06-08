import { AccountBaseAvatar } from "./AccountBaseAvatar"

export type CommentBase = {
  id: string
  creatorAccount: AccountBaseAvatar
  text: string
  created: number
  rating?: number
}
