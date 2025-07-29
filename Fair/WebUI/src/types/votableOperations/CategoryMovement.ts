import { BaseVotableOperation } from "./BaseVotableOperation"

export type CategoryMovement = {
  categoryId: string
  parentCategoryId: string
} & BaseVotableOperation
