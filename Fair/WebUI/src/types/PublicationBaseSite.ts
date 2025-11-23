import { AccountBaseAvatar } from "./AccountBaseAvatar"
import { PublicationImageBase } from "./PublicationImageBase"

export type PublicationBaseSite = {
  id: string
  publication: PublicationImageBase
  author: AccountBaseAvatar
}
