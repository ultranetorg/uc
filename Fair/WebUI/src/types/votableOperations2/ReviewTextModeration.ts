import { BaseVotableOperation } from "./BaseVotableOperation"

export type ReviewTextModeration = {
  reviewId: string
  hash: string
  resolution: boolean
} & BaseVotableOperation
