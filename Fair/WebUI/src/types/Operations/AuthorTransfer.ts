import { Operation } from "./Operation"

export type AuthorTransfer = {
  author: string
  to: string
} & Operation
