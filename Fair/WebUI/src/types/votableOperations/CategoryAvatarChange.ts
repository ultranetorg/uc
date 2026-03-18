import { BaseVotableOperation } from "./BaseVotableOperation"

export type CategoryAvatarChange = {
  categoryId: string
  categoryTitle: string
  fileId: string
} & BaseVotableOperation
