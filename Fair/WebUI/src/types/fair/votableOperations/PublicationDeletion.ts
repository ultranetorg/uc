import { BaseVotableOperation } from "./BaseVotableOperation"

export type PublicationDeletion = {
  publicationId: string
} & BaseVotableOperation
