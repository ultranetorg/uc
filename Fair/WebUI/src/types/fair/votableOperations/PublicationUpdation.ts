import { BaseVotableOperation } from "./BaseVotableOperation"

export type PublicationUpdation = {
  publicationId: string
  version: number
} & BaseVotableOperation
