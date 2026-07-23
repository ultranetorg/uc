import { AccountBaseAvatar } from "./AccountBaseAvatar"

export type CommentBase = {
  id: string
  creatorUser: AccountBaseAvatar
  text: string
  created: number
  rating?: number
}
