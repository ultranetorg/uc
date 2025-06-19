import { BaseVotableOperation } from "./BaseVotableOperation"

export type PublicationProductChange = {
  publicationId: string
  productId: string
} & BaseVotableOperation
