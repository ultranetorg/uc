import { AccountBase } from "./AccountBase"
import { PublicationImageBase } from "./PublicationImageBase"

export type PublicationBaseSite = {
  id: string
  publication: PublicationImageBase
  author: AccountBase
}
