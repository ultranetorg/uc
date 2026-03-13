import { AuthorBaseAvatar } from "./AuthorBaseAvatar"

export type Publisher = {
  author: AuthorBaseAvatar
  bannedTill: number
  energyLimit: number
  spacetimeLimit: number
}
