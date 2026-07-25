import { CategoryBase } from "types"
import { SimpleMenuItem } from "ui/components/SimpleMenu"
import { routes } from "utils"

export const toSimpleMenuItems = (categories: CategoryBase[], storeId: string): SimpleMenuItem[] =>
  categories.map(x => ({ to: routes.category(storeId, x.id), label: x.title }))
