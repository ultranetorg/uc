import { AuthorBase } from "./AuthorBase"
import { PublicationBase } from "./PublicationBase"

export type SiteAuthor = {
  ownerId: string
  publications?: PublicationBase[]
} & AuthorBase
