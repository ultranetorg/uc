import { AccountBase } from "./AccountBase"
import { SiteBase } from "./SiteBase"

export type Account = {
  favoriteSites: SiteBase[]
} & AccountBase
