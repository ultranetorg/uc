import { CategoryBase } from "./CategoryBase"
import { PublicationBase } from "./PublicationBase"

export type Category = {
  siteId: string
  parentId: string
  parentTitle: string
  categories?: CategoryBase[]
  publications?: PublicationBase[]
} & CategoryBase
