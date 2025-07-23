import { PublicationBase } from "./PublicationBase"

export type PublicationAuthor = {
  logo?: string
  publicationsCount: number
} & PublicationBase
