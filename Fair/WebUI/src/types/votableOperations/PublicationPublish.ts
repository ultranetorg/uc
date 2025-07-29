import { BaseVotableOperation } from "./BaseVotableOperation"

export type PublicationPublish = {
  publicationId: string
  categoryId: string
} & BaseVotableOperation
