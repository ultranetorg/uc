import { memo } from "react"

import { ThreeDotsMenu } from "ui/components"

import { useCategoryContextThreeDotsMenu } from "./CategoryTreeContextThreeDotsMenu"

export type CategoryThreeDotsMenuProps = {
  categoryId: string
  categoryTitle: string
}

export const CategoryThreeDotsMenu = memo(({ categoryId, categoryTitle }: CategoryThreeDotsMenuProps) => {
  const menuItems = useCategoryContextThreeDotsMenu(categoryId, categoryTitle)

  return <ThreeDotsMenu items={menuItems} />
})
