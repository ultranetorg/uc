export const publicationsKeys = {
  store: (storeId: string) => ["stores", storeId] as const,

  authorPublications: (storeId: string, authorId: string, page?: number, pageSize?: number) =>
    [...publicationsKeys.store(storeId), "authors", authorId, { page, pageSize }] as const,
  categoriesPublications: (storeId: string) =>
    [...publicationsKeys.store(storeId), "categories", "publication"] as const,

  categoryPublications: (categoryId: string, page?: number) => ["categories", categoryId, { page }] as const,

  detail: (publicationId: string) => ["publications", publicationId] as const,
  diff: (publicationId: string, version: number) =>
    [...publicationsKeys.detail(publicationId), "diff", { version }] as const,
  lastVersion: (publicationId: string) => [...publicationsKeys.detail(publicationId), "versions", "latest"] as const,
}
