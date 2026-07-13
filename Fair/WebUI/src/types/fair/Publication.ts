import { PublicationBase } from "./PublicationBase"

export type Publication = {
  logoFileId?: string
  categoryId: string
  categoryTitle: string
  rating: number
} & PublicationBase
