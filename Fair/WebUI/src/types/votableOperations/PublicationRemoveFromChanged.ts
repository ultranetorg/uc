import { BaseVotableOperation } from "./BaseVotableOperation"

export type PublicationRemoveFromChangedModel = {
  publicationId: string
} & BaseVotableOperation
