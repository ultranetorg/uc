import { StoreBase } from "./StoreBase"
import { User } from "./User"

export type UserDetails = {
  favoriteStores: StoreBase[]
  authorsIds: string[]
  hasAvatar: boolean
} & User
