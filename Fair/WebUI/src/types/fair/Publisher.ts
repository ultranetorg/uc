import { AuthorBaseAvatar } from "./AuthorBaseAvatar"

export type Publisher = {
  author: AuthorBaseAvatar
  verifiedWebdomainRank?: number
  bannedTill: number
  energyLimit: number
  spacetimeLimit: number
}
