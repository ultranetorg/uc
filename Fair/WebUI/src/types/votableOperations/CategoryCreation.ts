import { BaseVotableOperation } from "./BaseVotableOperation"

export type CategoryCreation = {
  parentCategoryId?: string
  parentCategoryTitle?: string
  title: string
} & BaseVotableOperation
