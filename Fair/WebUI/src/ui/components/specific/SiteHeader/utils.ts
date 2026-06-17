import { CategoryBase } from "types"
import { SimpleMenuItem } from "ui/components/SimpleMenu"
import { routes } from "utils"

export const toSimpleMenuItems = (categories: CategoryBase[], siteId: string): SimpleMenuItem[] =>
  categories.map(x => ({ to: routes.category(siteId, x.id), label: x.title }))
