import { BaseVotableOperation } from "./BaseVotableOperation"

export type CategoryCreation = {
  parentCategoryId: string
  title: string
} & BaseVotableOperation
