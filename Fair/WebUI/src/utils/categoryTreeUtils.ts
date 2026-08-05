import { Category, CategoryBase, CategoryParentBase, CategoryParentBaseWithChildren } from "types"

export type CategoryTreeItem = {
  id: string
  title: string
  depth: number
  active?: boolean
}

export const buildCategoryPathItems = (category: Category, siblings?: CategoryBase[]): CategoryTreeItem[] => {
  const ancestors = category.path ?? []
  const ancestorItems = ancestors.map((item, index) => ({ id: item.id, title: item.title, depth: index }))

  if (category.categories.length > 0) {
    return [
      ...ancestorItems,
      { id: category.id, title: category.title, depth: ancestors.length, active: true },
      ...category.categories.map(item => ({ id: item.id, title: item.title, depth: ancestors.length + 1 })),
    ]
  }

  // Category has no children of its own: keep its siblings visible and highlight it among them,
  // instead of collapsing the level down to a single active leaf.
  const levelItems = siblings && siblings.length > 0 ? siblings : [category]

  return [
    ...ancestorItems,
    ...levelItems.map(item => ({
      id: item.id,
      title: item.title,
      depth: ancestors.length,
      active: item.id === category.id,
    })),
  ]
}

export const buildRootCategoryItems = (categories: CategoryBase[]): CategoryTreeItem[] =>
  categories.map(item => ({ id: item.id, title: item.title, depth: 0 }))

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
