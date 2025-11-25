import { PublicationBase } from "./PublicationBase"

export type PublicationImageBase = {
  categoryTitle: string
  imageId?: string
} & PublicationBase
