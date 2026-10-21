import { RoleName } from "./RoleName"

export type BaseComment = {
  id: string
  text: string

  creatorId: string
  creatorName: string
  creatorAs: RoleName
  creatorAvatarId?: string

  created: number
}
