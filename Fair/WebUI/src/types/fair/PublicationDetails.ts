import { ProductDetails } from "./ProductDetails"

export type PublicationDetails = {
  siteId: string
  categoryId?: string
  categoryTitle?: string
  rating?: number
} & ProductDetails
