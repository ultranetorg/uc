import { FieldValue } from "./ProductField"
import { ProductType } from "./ProductType"

export type ProductDetails = {
  id: string

  type: ProductType

  title?: string
  logoId?: string
  updated: number

  authorId: string
  authorTitle: string
  authorLogoId?: string

  fields: FieldValue[]
}
