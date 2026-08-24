import { CategoryBase, CategoryParentBase, CategoryParentBaseWithChildren, ProductType } from "types"

export type CategoryTreeItem = {
  id: string
  title: string
  avatarId?: string
  depth: number
  active?: boolean
  expanded?: boolean
  hasChildren?: boolean
}

export const buildRootCategoryItems = (categories: CategoryBase[]): CategoryTreeItem[] =>
  categories.map(item => ({ id: item.id, title: item.title, avatarId: item.avatarId, depth: 0 }))

const findCategoryPath = (
  categories: CategoryParentBaseWithChildren[],
  categoryId: string,
): CategoryParentBaseWithChildren[] | undefined => {
  for (const category of categories) {
    if (category.id === categoryId) {
      return [category]
    }

    const childPath = findCategoryPath(category.children, categoryId)
    if (childPath) {
      return [category, ...childPath]
    }
  }

  return undefined
}

// A category without children has nothing to expand into, so the tree stays expanded down to its
// closest ancestor that has children, keeping that ancestor's subcategories visible.
const takeExpandablePath = (path: CategoryParentBaseWithChildren[]): CategoryParentBaseWithChildren[] => {
  for (let index = path.length - 1; index >= 0; --index) {
    if (path[index].children.length > 0) {
      return path.slice(0, index + 1)
    }
  }

  return path
}

export const buildCategoryTreeItems = (
  categories: CategoryParentBaseWithChildren[],
  activeCategoryId?: string,
): CategoryTreeItem[] => {
  const path = activeCategoryId ? findCategoryPath(categories, activeCategoryId) : undefined
  const expandedIds = new Set(path ? takeExpandablePath(path).map(item => item.id) : [])

  const build = (items: CategoryParentBaseWithChildren[], depth: number): CategoryTreeItem[] =>
    items.flatMap(item => {
      const hasChildren = item.children.length > 0
      const expanded = expandedIds.has(item.id) && hasChildren

      return [
        {
          id: item.id,
          title: item.title,
          avatarId: item.avatarId,
          depth,
          active: item.id === activeCategoryId,
          expanded,
          hasChildren,
        },
        ...(expanded ? build(item.children, depth + 1) : []),
      ]
    })

  return build(categories, 0)
}

export type CategoryTree = {
  tree: CategoryParentBaseWithChildren[]
  types: ProductType[]
}

export const buildCategoryTreeAndTypes = (categories: CategoryParentBase[]): CategoryTree => {
  const map = new Map<string, CategoryParentBaseWithChildren>()
  const types = new Set<ProductType>()

  for (const category of categories) {
    map.set(category.id, { ...category, children: [] })

    if (category.type !== "none") types.add(category.type)
  }

  const tree: CategoryParentBaseWithChildren[] = []

  for (const category of map.values()) {
    if (category.parentId && map.has(category.parentId)) {
      const parent = map.get(category.parentId)!
      parent.children.push(category)
    } else {
      tree.push(category)
    }
  }

  return { tree, types: Array.from(types) }
}
