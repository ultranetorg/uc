export const categoriesKeys = {
  all: ["categories"] as const,
  detail: (categoryId: string) => [...categoriesKeys.all, categoryId] as const,
  siteCategories: (siteId: string, depth?: number) => ["sites", siteId, "categories", { depth }],
}
