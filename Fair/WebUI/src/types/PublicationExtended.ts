import { Publication } from "./Publication"

export type PublicationExtended = {
  authorId: string
  authorTitle: string
  categoryId: string
  categoryTitle: string
} & Publication
