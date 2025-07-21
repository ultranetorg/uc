import { PublicationBase } from "./PublicationBase"

export type Publication = {
  logo: string
  categoryId: string
  categoryTitle: string
} & PublicationBase
