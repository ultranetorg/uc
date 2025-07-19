import { AuthorBase } from "./AuthorBase"

export type AuthorDetails = {
  avatar: string
  ownersIds: string[]
  description: string
} & AuthorBase
