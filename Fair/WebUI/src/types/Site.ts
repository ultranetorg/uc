import { SiteBase } from "./SiteBase"
import { SiteCategory } from "./SiteCategory"

export type Site = {
  categories: SiteCategory[]
} & SiteBase
