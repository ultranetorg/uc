import { AuthorBase } from "./AuthorBase"

export type Author = {
  ownersIds: string[]
} & AuthorBase
