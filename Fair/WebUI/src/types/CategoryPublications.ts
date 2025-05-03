import { CategoryBase } from "./CategoryBase"
import { PublicationExtended } from "./PublicationExtended"

export type CategoryPublications = {
  publications: PublicationExtended[]
} & CategoryBase
