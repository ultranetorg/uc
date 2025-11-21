import { ProductFieldModel } from "./ProductField"
import { ProductType } from "./ProductType"

export type Product = {
  id: string
  type: ProductType
  versions: ProductFieldModel[]
}
