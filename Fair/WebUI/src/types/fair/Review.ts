import { BaseComment } from "./BaseComment"

export type Review = BaseComment & {
  rating: number
  publicationId: string
  publicationTitle: string
}
