import { Publication } from "./Publication"

export type PublicationSearch = {
  authorId: string
  authorTitle: string
  categoryId: string
  categoryTitle: string
} & Publication
