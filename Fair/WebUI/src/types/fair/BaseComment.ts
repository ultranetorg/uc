import { Creator } from "./Creator"

export type BaseComment = {
  id: string
  text: string

  creator: Creator

  created: number
}
