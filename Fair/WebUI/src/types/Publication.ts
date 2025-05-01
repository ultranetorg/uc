import { PublicationBase } from "./PublicationBase"

export type Publication = {
  supportedOSes: string[]
  averageRating: number
} & PublicationBase
