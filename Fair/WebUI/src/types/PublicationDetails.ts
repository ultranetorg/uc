import { ProductDetails } from "./ProductDetails"

export type PublicationDetails = {
  categoryId?: string
  categoryTitle?: string
  rating?: number
} & ProductDetails
