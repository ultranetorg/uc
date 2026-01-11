import { ProductType } from "./ProductType"
import { PublicationExtended } from "./PublicationExtended"
import { ProductFieldModel } from "./ProductField"

export type PublicationDetails = {
  productType: ProductType
  authorAvatar: string
  rating: number
  reviewsCount: number
  description: string
  productUpdated: number
  productFields?: ProductFieldModel[]
} & PublicationExtended
