import { Category, CategoryBase, CategoryParentBase, CategoryParentBaseWithChildren } from "types"

export type CategoryTreeItem = {
  id: string
  title: string
  depth: number
  active?: boolean
}

export const buildCategoryPathItems = (category: Category): CategoryTreeItem[] => {
  const ancestors = category.path ?? []

  return [
    ...ancestors.map((item, index) => ({ id: item.id, title: item.title, depth: index })),
    { id: category.id, title: category.title, depth: ancestors.length, active: true },
    ...category.categories.map(item => ({ id: item.id, title: item.title, depth: ancestors.length + 1 })),
  ]
}

export const buildRootCategoryItems = (categories: CategoryBase[]): CategoryTreeItem[] =>
  categories.map(item => ({ id: item.id, title: item.title, depth: 0 }))

export const buildCategoryTreeItems = (rootCategories: CategoryBase[], category: Category): CategoryTreeItem[] => {
  const pathItems = buildCategoryPathItems(category)
  const activeRootId = category.path?.[0]?.id ?? category.id
  const hasActiveRoot = rootCategories.some(item => item.id === activeRootId)

  const items = rootCategories.flatMap(item =>
    item.id === activeRootId ? pathItems : [{ id: item.id, title: item.title, depth: 0 }],
  )

  return hasActiveRoot ? items : [...items, ...pathItems]
}

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
