import { BaseVotableOperation } from "./BaseVotableOperation"
import { ProductFieldVersionReference } from "../ProductFieldVersionReference"

export type PublicationUpdation = {
  publicationId: string
  change: ProductFieldVersionReference
  resolution: boolean
} & BaseVotableOperation
