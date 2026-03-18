import { BaseVotableOperation } from "./BaseVotableOperation"

export type CategoryDeletion = {
  categoryId: string
  categoryTitle: string
} & BaseVotableOperation
