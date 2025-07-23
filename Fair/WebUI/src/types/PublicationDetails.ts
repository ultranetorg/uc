import { PublicationExtended } from "./PublicationExtended"

export type PublicationDetails = {
  authorAvatar: string
  rating: number
  reviewsCount: number
  description: string
  productUpdated: number
} & PublicationExtended
