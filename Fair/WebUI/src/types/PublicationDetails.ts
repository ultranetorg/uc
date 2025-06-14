import { PublicationExtended } from "./PublicationExtended"

export type PublicationDetails = {
  averageRating: number
  reviewsCount: number
  creatorId: string
  description: string
  productUpdated: number
} & PublicationExtended
