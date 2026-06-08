import { CommentBase } from "./CommentBase"

export type Review = CommentBase & {
  rating: number
  publicationId: string
  publicationTitle: string
}
