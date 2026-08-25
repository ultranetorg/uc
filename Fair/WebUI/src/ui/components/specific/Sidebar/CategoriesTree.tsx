import { memo } from "react"

import { CategoryTreeItem as CategoryTreeItemType } from "utils"
import { CategoryTreeItem } from "./CategoryTreeItem"
import { CategoryTreeRootItem } from "./CategoryTreeRootItem"

export type CategoriesTreeProps = {
  storeId: string
  items: CategoryTreeItemType[]
}

export const CategoriesTree = memo(({ storeId, items }: CategoriesTreeProps) => {
  return (
    <div className="flex flex-col">
      {items.map(x =>
        x.depth === 0 ? (
          <CategoryTreeRootItem key={x.id} storeId={storeId} {...x} />
        ) : (
          <CategoryTreeItem key={x.id} storeId={storeId} {...x} />
        ),
      )}
    </div>
  )
})
