import { CategoryPathItem } from "./CategoryPathItem"
import { ProductDetails } from "./ProductDetails"

export type PublicationDetails = {
  storeId: string
  path?: CategoryPathItem[]
  rating?: number
} & ProductDetails
