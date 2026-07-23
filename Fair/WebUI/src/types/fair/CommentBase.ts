import { UserBaseAvatar } from "./UserBaseAvatar"

export type CommentBase = {
  id: string
  creatorUser: UserBaseAvatar
  text: string
  created: number
  rating?: number
}
