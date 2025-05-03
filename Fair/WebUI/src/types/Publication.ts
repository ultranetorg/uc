import { PublicationBase } from "./PublicationBase"

export type Publication = {
  // TODO: should be removed in Books, Movies etc
  supportedOSes: string[]
  averageRating: number
} & PublicationBase
