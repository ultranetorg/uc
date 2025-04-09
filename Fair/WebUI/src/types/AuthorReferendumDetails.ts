import { AuthorReferendum } from "./AuthorReferendum"

export type AuthorReferendumDetails = {
  pros: string[]
  cons: string[]
  abs: string[]
} & AuthorReferendum
