import { StoreBase } from "./StoreBase"
import { User } from "./User"

export type UserDetails = {
  favoriteSites: StoreBase[]
  authorsIds: string[]
  hasAvatar: boolean
} & User
