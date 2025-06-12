import { TestCategoryIcon } from "testConstants"
import { CategoryBase } from "types"
import { BigCategoriesListItem } from "ui/components"

export const toBigCategoriesListItems = (categories: CategoryBase[]): BigCategoriesListItem[] =>
  categories.map(x => ({ id: x.id, title: x.title, icon: <TestCategoryIcon /> }))
