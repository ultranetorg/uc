import { ProductField } from "./ProductField"

export type Product = {
  id: string
  name: string
  authorId: string
  authorName: string
  flags: string[]
  fields: ProductField[]
  updated: number
}
