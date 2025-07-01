import { AuthorBase } from "./AuthorBase"

export type AuthorExtended = {
  publicationsCount: number
} & AuthorBase
