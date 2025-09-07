import { CommentBase } from "./CommentBase"

export type Review = CommentBase & {
  rating: number
}
