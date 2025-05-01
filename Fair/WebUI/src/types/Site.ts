import { AccountBase } from "./AccountBase"
import { CategoryBase } from "./CategoryBase"
import { SiteBase } from "./SiteBase"

export type Site = {
  moderators: AccountBase[]
  categories: CategoryBase[]
} & SiteBase
