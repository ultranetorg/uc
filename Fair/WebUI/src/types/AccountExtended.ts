import { AccountBase } from "./AccountBase"

export type AccountExtended = {
  roles: string[]
  registrationDate: number
} & AccountBase
