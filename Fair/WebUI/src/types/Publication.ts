import { PublicationBase } from "./PublicationBase"

export type Publication = {
  logoFileId?: string
  logo: string // TODO: should be removed!
  categoryId: string
  categoryTitle: string
} & PublicationBase
