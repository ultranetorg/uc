import { BaseVotableOperation } from "./BaseVotableOperation"

export type CategoryTypeChange = {
  categoryId: string
  type: string
} & BaseVotableOperation
