import { AccountBase } from "./AccountBase"

export type Moderator = {
  user: AccountBase
  bannedTill: number
}
