import { BaseVotableOperation } from "./BaseVotableOperation"

export type CategoryAvatarChange = {
  categoryId: string
  fileId: string
} & BaseVotableOperation
