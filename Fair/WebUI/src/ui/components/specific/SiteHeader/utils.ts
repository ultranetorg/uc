import { CategoryBase } from "types"
import { SimpleMenuItem } from "ui/components/SimpleMenu"

export const toSimpleMenuItems = (categories: CategoryBase[], siteId: string): SimpleMenuItem[] =>
  categories.map(x => ({ to: `/${siteId}/c/${x.id}`, label: x.title }))
