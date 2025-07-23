import { AuthorBase } from "./AuthorBase"

export type AuthorDetails = {
  description: string
  avatar: string
  ownersIds: string[]
  links: string[]
} & AuthorBase
