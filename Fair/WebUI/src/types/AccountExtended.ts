import { AccountBaseAvatar } from "./AccountBaseAvatar"

export type AccountExtended = {
  roles: string[]
  registrationDate: number
} & AccountBaseAvatar
