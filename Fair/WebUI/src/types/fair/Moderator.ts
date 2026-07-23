import { UserBase } from "./UserBase"

export type Moderator = {
  user: UserBase
  bannedTill: number
}
