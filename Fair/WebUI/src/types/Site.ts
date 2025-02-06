import { Account } from "./Account"
import { CategoryBase } from "./CategoryBase"
import { SiteBase } from "./SiteBase"

export type Site = {
  moderators?: Account[]
  categories?: CategoryBase[]
} & SiteBase
