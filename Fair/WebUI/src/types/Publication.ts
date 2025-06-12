import { PublicationBase } from "./PublicationBase"

export type Publication = {
  categoryId: string
  categoryTitle: string
} & PublicationBase
