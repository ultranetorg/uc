import { BaseVotableOperation } from "./BaseVotableOperation"

export type ReviewEditModeration = {
  reviewId: string
  hash: string
  resolution: boolean
} & BaseVotableOperation
