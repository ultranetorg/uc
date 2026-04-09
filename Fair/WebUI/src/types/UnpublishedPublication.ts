import { ProductType } from "./ProductType"

// TODO: should be refactored and replaced with Publication
export type UnpublishedPublication = {
  id: string

  type: ProductType

  title?: string
  logoId?: string
  updated: number

  authorId: string
  authorTitle: string
  authorLogoId?: string
}
