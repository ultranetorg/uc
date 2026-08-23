import { memo } from "react"

import { CategoryTreeItem } from "utils"
import { CategoryTreeRootItem } from "./CategoryTreeRootItem"

export type CategoriesTreeProps = {
  storeId: string
  items: CategoryTreeItem[]
}

export const CategoriesTree = memo(({ storeId, items }: CategoriesTreeProps) => {
  return (
    <div className="flex flex-col gap-1">
      {items.map(x => (
        <CategoryTreeRootItem {...x} />
      ))}
    </div>
  )
})
