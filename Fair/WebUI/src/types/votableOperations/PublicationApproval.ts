import { BaseVotableOperation } from "./BaseVotableOperation"

export type PublicationApproval = {
  publicationId: string
} & BaseVotableOperation
