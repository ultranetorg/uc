export const unpublishedPublicationsKeys = {
  all: (siteId: string) => ["sites", siteId, "publications", "unpublished"] as const,
  paged: (siteId: string, page?: number, pageSize?: number) =>
    [...unpublishedPublicationsKeys.all(siteId), { page, pageSize }] as const,
  detail: (siteId: string, unpublishedPublicationId: string) =>
    [...unpublishedPublicationsKeys.all(siteId), unpublishedPublicationId] as const,
}
