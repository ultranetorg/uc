import { ProductField } from "./ProductField"
import { PublicationBase } from "./PublicationBase"

export type Publication = {
  categoryId: string
  creatorId: string
  productFields: ProductField[]
  productUpdated: number
  authorId: string
  authorTitle: string
} & PublicationBase
