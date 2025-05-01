import { PublicationSearch } from "./PublicationSearch"

export type PublicationDetails = {
  reviewsCount: number
  creatorId: string
  description: string
  productUpdated: number
} & PublicationSearch
