import { BaseVotableOperation } from "./BaseVotableOperation"

export type PublicationPublish = {
  publicationId: string
  publicationTitle: string
  categoryId: string
  categoryTitle: string
} & BaseVotableOperation
