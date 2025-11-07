import { PublicationBaseSite } from "./PublicationBaseSite"

export type PublicationChanged = {
  categoryId: string
  categoryTitle: string
  currentVersion: number
  latestVersion: number
} & PublicationBaseSite
