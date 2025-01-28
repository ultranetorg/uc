import { CategorySubModel } from "./CategorySubModel"

export type Site = {
  id: string
  type: string
  title: string
  categories: CategorySubModel[]
}
