export const publicationsKeys = {
  site: (siteId: string) => ["sites", siteId] as const,

  authorPublications: (siteId: string, authorId: string, page?: number, pageSize?: number) =>
    [...publicationsKeys.site(siteId), "authors", authorId, { page, pageSize }] as const,
  categoriesPublications: (siteId: string) => [...publicationsKeys.site(siteId), "categories", "publication"] as const,

  categoryPublications: (categoryId: string, page?: number) => ["categories", categoryId, { page }] as const,

  detail: (publicationId: string) => ["publications", publicationId] as const,
  diff: (publicationId: string, version: number) =>
    [...publicationsKeys.detail(publicationId), "diff", { version }] as const,
  lastVersion: (publicationId: string) => [...publicationsKeys.detail(publicationId), "versions", "latest"] as const,
}
