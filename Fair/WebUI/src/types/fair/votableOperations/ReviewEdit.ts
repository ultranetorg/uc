import { BaseVotableOperation } from "./BaseVotableOperation"

export type ReviewEdit = {
  reviewId: string
  text: string
} & BaseVotableOperation
