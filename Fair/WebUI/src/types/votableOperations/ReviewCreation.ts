import { BaseVotableOperation } from "./BaseVotableOperation"

export type ReviewCreation = {
  publicationId: string
  text: string
  rating: number
} & BaseVotableOperation
