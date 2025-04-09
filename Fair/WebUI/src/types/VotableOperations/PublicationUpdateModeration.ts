import { BaseVotableOperation } from "./BaseVotableOperation"
import { ProductFieldVersionReference } from "../ProductFieldVersionReference"

export type PublicationUpdateModeration = {
  publicationId: string
  change: ProductFieldVersionReference
  resolution: boolean
} & BaseVotableOperation
