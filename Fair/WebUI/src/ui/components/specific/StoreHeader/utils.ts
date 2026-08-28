import { CategoryBase } from "types"
import { SimpleMenuItem } from "ui/components/SimpleMenu"
import { routes } from "utils"

export const toSimpleMenuItems = (categories: CategoryBase[]): SimpleMenuItem[] =>
  categories.map(x => ({ to: routes.category(x.id), label: x.title }))
