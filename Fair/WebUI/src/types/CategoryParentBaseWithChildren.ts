import { CategoryParentBase } from "types"

export type CategoryParentBaseWithChildren = CategoryParentBase & {
  children: CategoryParentBaseWithChildren[]
}
