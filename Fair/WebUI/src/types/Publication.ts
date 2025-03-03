import { ProductField } from "./ProductField"
import { PublicationBase } from "./PublicationBase"
import { Review } from "./Review"

export type Publication = {
  categoryId: string
  creatorId: string
  productFields: ProductField[]
  productUpdated: number
  authorId: string
  authorTitle: string
  reviews?: Review[]
} & PublicationBase
