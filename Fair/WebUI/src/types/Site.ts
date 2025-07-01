import { CategoryBase } from "./CategoryBase"
import { SiteBase } from "./SiteBase"

export type Site = {
  categories: CategoryBase[]
} & SiteBase
