import { ProductType } from "types"

import { BaseVotableOperation } from "./BaseVotableOperation"

export type CategoryTypeChange = {
  categoryId: string
  type: ProductType
} & BaseVotableOperation
