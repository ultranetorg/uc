export const categoriesKeys = {
  all: (storeId: string) => ["stores", storeId, "categories"] as const,
  root: (storeId: string) => [...categoriesKeys.all(storeId), "root"] as const,
  details: () => ["categories", "detail"] as const,
  detail: (categoryId: string) => [...categoriesKeys.details(), categoryId] as const,
  tree: (storeId: string, depth?: number) => [...categoriesKeys.all(storeId), "tree", { depth }] as const,
}
