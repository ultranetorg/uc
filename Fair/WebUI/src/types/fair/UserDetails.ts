import { SiteBase } from "./SiteBase"
import { User } from "./User"

export type UserDetails = {
  favoriteSites: SiteBase[]
  authorsIds: string[]
  hasAvatar: boolean
} & User
