import { AccountBaseAvatar } from "./AccountBaseAvatar"
import { ProductType } from "./ProductType"
import { PublicationImageBase } from "./PublicationImageBase"

export type UnpublishedProduct = {
  id: string
  type: ProductType
  publication: PublicationImageBase
  author: AccountBaseAvatar
}
