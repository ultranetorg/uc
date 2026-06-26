import { CategoryBase } from "./CategoryBase"
import { ProductType } from "./ProductType"

export type Category = {
  siteId: string
  type: ProductType
  parentId: string
  parentTitle: string
  categories: CategoryBase[]
  publicationsCount: number
} & CategoryBase
