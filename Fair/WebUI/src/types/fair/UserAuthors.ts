import { AuthorBaseAvatar } from "./AuthorBaseAvatar"
import { User } from "./User"

export type UserAuthors = {
  authors: AuthorBaseAvatar[]
} & User
