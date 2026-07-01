import { CategoryBase } from "./CategoryBase"
import { CategoryPathItem } from "./CategoryPathItem"
import { ProductType } from "./ProductType"

export type Category = {
  siteId: string
  type: ProductType
  parentId: string
  path?: CategoryPathItem[]
  categories: CategoryBase[]
  publicationsCount: number
} & CategoryBase
