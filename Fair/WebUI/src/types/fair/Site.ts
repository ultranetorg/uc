import { SiteBase } from "./SiteBase"

export type Site = {
  authorsIds: string[]
  moderatorsIds: string[]
} & SiteBase
