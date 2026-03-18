import { BaseVotableOperation } from "./BaseVotableOperation"

export type CategoryMovement = {
  categoryId: string
  categoryTitle: string
  parentCategoryId?: string
  parentCategoryTitle?: string
} & BaseVotableOperation
