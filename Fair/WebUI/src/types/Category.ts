import { CategoryPublicationModel } from "./CategoryPublicationModel"
import { CategorySubModel } from "./CategorySubModel"

export type Category = {
  id: string
  title: string
  siteId: string
  parentId: string
  parentTitle: string
  categories: CategorySubModel[]
  publications: CategoryPublicationModel[]
}
