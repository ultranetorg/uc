import { CategoryBase } from "./CategoryBase"
import { ProductType } from "./ProductType"

export type CategoryParentBase = {
  parentId: string
  type: ProductType
} & CategoryBase
