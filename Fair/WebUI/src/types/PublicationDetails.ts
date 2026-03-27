import { ProductType } from "./ProductType"
import { PublicationExtended } from "./PublicationExtended"
import { FieldValue } from "./ProductField"

export type PublicationDetails = {
  productType: ProductType
  authorAvatar: string
  rating: number
  reviewsCount: number
  description: string
  productUpdated: number
  productFields?: FieldValue[]
} & PublicationExtended
