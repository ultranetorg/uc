import { PublicationExtended } from "./PublicationExtended"

export type PublicationDetails = {
  reviewsCount: number
  creatorId: string
  description: string
  productUpdated: number
} & PublicationExtended
