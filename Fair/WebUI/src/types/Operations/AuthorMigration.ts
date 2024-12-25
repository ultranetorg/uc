import { Operation } from "./Operation"

export type AuthorMigration = {
  author: string
  tld: string
  rankCheck: boolean
} & Operation
