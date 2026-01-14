import { ProductField } from "./ProductField"
import { UnpublishedProduct } from "./UnpublishedProduct"

export type UnpublishedProductDetails = {
  title: string
  description: string
  logoId: string
  versions: ProductField[]
} & UnpublishedProduct
