import { CategoryBase } from "./CategoryBase"
import { CategoryType } from "./CategoryType"
import { PublicationExtended } from "./PublicationExtended"

export type CategoryPublications = {
  type: CategoryType
  publications: PublicationExtended[]
} & CategoryBase
