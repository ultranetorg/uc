import { BaseVotableOperation } from "./BaseVotableOperation"

export type PublicationStatusChange = {
  publicationId: string
  status: string
} & BaseVotableOperation
