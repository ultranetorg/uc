import { ProductFieldModel } from "./ProductField"
import { ProductType } from "./ProductType"

export type Product = {
  id: string
  type: ProductType
  title?: string
  description?: string
  logoId?: string
  versions: ProductFieldModel[]
}
