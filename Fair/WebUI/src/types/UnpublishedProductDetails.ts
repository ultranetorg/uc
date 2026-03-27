import { FieldValue } from "./ProductField"
import { UnpublishedProduct } from "./UnpublishedProduct"

export type UnpublishedProductDetails = {
  title: string
  description: string
  logoId: string
  fields: FieldValue[]
} & UnpublishedProduct
