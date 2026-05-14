import { AuthorBase } from "./AuthorBase"
import { User } from "./User"

export type AuthorDetails = {
  description: string
  avatarId?: string
  ownersIds: User[]
  links: string[]
} & AuthorBase
