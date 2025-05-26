import { BaseVotableOperation } from "./BaseVotableOperation"

export type PublicationCategoryChange = {
  publicationId: string
  categoryId: string
} & BaseVotableOperation
