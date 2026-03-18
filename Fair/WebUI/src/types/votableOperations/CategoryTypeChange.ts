import { ProductType } from "types"

import { BaseVotableOperation } from "./BaseVotableOperation"

export type CategoryTypeChange = {
  categoryId: string
  categoryTitle: string
  categoryType: ProductType
  type: ProductType
} & BaseVotableOperation
