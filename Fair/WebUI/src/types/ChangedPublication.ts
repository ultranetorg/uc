import { AccountBaseAvatar } from "./AccountBaseAvatar"
import { PublicationImageBase } from "./PublicationImageBase"

export type ChangedPublication = {
  id: string
  productId: string
  publication: PublicationImageBase
  author: AccountBaseAvatar
  categoryId: string
  categoryTitle: string
  currentVersion: number
  latestVersion: number
}
