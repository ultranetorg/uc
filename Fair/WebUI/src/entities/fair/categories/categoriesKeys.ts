export const categoriesKeys = {
  all: (siteId: string) => ["sites", siteId, "categories"] as const,
  root: (siteId: string) => [...categoriesKeys.all(siteId), "root"] as const,
  detail: (siteId: string, categoryId: string) => [...categoriesKeys.all(siteId), categoryId] as const,
  tree: (siteId: string, depth?: number) => [...categoriesKeys.all(siteId), "tree", { depth }] as const,
}
