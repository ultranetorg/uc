import { ProductType } from "./ProductType"
import { Publication } from "./Publication"

export type PublicationExtended = {
  authorId: string
  authorTitle: string
  authorFileId?: string
  type?: ProductType
} & Publication
