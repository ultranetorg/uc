import { BaseVotableOperation } from "./BaseVotableOperation"

export type ReviewStatusChange = {
  reviewId: string
  status: string
} & BaseVotableOperation
