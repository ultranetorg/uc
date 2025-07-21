import { PublicationExtended } from "./PublicationExtended"

export type PublicationDetails = {
  authorAvatar: string
  rating: number
  reviewsCount: number
  creatorId: string
  description: string
  productUpdated: number
} & PublicationExtended
