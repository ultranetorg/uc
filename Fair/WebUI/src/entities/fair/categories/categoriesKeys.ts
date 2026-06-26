export const categoriesKeys = {
  all: (siteId: string) => ["sites", siteId, "categories"] as const,
  root: (siteId: string) => [...categoriesKeys.all(siteId), "root"] as const,
  detail: (categoryId: string) => ["categories", categoryId] as const,
  tree: (siteId: string, depth?: number) => [...categoriesKeys.all(siteId), "tree", { depth }] as const,
}
