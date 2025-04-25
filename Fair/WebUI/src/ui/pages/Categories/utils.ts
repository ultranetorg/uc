import { CategoryParentBase } from "types"

import { CategoryParentBaseWithChildren } from "./types"

export const buildCategoryTree = (categories: CategoryParentBase[]): CategoryParentBaseWithChildren[] => {
  const map = new Map<string, CategoryParentBaseWithChildren>()

  for (const category of categories) {
    map.set(category.id, { ...category, children: [] })
  }

  const result: CategoryParentBaseWithChildren[] = []

  for (const category of map.values()) {
    if (category.parentId && map.has(category.parentId)) {
      const parent = map.get(category.parentId)!
      parent.children.push(category)
    } else {
      result.push(category)
    }
  }

  return result
}
