import { PublicationBase } from "./PublicationBase"

export type PublicationAuthor = {
  productId: string
  logoId?: string
  publicationsCount: number
  categoryId: string
  categoryTitle: string
} & PublicationBase
