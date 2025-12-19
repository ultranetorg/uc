import { ProductType } from "./ProductType"
import { ProductFieldModel } from "./ProductField"
import { PublicationExtended } from "./PublicationExtended"

export type PublicationDetails = {
  productType: ProductType
  authorAvatar: string
  rating: number
  reviewsCount: number
  description: string
  productUpdated: number

	productFields?: ProductFieldModel[]
} & PublicationExtended
