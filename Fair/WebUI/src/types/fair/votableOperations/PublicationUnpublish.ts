import { BaseVotableOperation } from "./BaseVotableOperation"

export type PublicationUnpublish = {
  publicationId: string
  publicationTitle: string
  categoryId: string
  categoryTitle: string
} & BaseVotableOperation
