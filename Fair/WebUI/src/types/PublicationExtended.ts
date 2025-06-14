import { Publication } from "./Publication"

export type PublicationExtended = {
  authorId: string
  authorTitle: string
} & Publication
