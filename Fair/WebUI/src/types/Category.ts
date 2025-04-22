import { CategoryBase } from "./CategoryBase"

export type Category = {
  parentId: string
  parentTitle: string
  categories: CategoryBase[]
  publicationsCount: number
} & CategoryBase
