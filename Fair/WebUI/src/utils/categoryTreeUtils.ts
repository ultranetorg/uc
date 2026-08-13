import { CategoryParentBase, CategoryParentBaseWithChildren, ProductType } from "types"

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
