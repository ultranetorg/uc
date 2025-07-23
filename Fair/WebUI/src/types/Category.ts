import { CategoryBase } from "./CategoryBase"
import { CategoryType } from "./CategoryType"

export type Category = {
  type: CategoryType
  parentId: string
  parentTitle: string
  categories: CategoryBase[]
  publicationsCount: number
} & CategoryBase
