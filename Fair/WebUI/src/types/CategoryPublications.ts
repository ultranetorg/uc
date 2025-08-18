import { CategoryBase } from "./CategoryBase"
import { ProductType } from "./ProductType"
import { PublicationExtended } from "./PublicationExtended"

export type CategoryPublications = {
  type: ProductType
  publications: PublicationExtended[]
} & CategoryBase
