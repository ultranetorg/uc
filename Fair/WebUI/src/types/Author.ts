import { AuthorBase } from "./AuthorBase"

export type Author = {
  ownersIds: string[]
  description: string
} & AuthorBase
