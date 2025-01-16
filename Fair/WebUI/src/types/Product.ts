import { ProductField } from "./ProductField"

export type Product = {
  id: string
  authorId: string
  flags: string[]
  fields: ProductField[]
  updated: number
}
