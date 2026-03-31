import { FieldValue } from "./ProductField"
import { ProductType } from "./ProductType"

export type ProductDetails = {
  id: string
  productType: ProductType

  title?: string
  description?: string
  logoFileId?: string
  updated: number

  authorId: string
  authorTitle: string
  authorFileId?: string

  productFields: FieldValue[]
}
