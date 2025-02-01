import { ProductField } from "./ProductField"

export type Publication = {
  id: string
  categoryId: string
  creatorId: string
  productId: string
  productName: string
  productFields: ProductField[]
  productUpdated: number
  productAuthorId: string
  productAuthorTitle: string
  sections: string[]
  comments: Comment[]
}
