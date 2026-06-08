import { ReviewStatus } from "types"

import { BaseVotableOperation } from "./BaseVotableOperation"

export type ReviewStatusChange = {
  reviewId: string
  status: ReviewStatus
} & BaseVotableOperation
