import { CategoryPathItem } from "./CategoryPathItem"
import { ProductDetails } from "./ProductDetails"

export type PublicationDetails = {
  siteId: string
  path?: CategoryPathItem[]
  rating?: number
} & ProductDetails
