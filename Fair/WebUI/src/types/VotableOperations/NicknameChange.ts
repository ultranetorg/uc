import { BaseVotableOperation } from "./BaseVotableOperation"

export type NicknameChange = {
  nickname: string
  field: string
  entityId: string
} & BaseVotableOperation
