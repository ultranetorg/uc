export const unpublishedPublicationsKeys = {
  all: (storeId: string) => ["stores", storeId, "publications", "unpublished"] as const,
  paged: (storeId: string, page?: number, pageSize?: number) =>
    [...unpublishedPublicationsKeys.all(storeId), { page, pageSize }] as const,
  detail: (storeId: string, unpublishedPublicationId: string) =>
    [...unpublishedPublicationsKeys.all(storeId), unpublishedPublicationId] as const,
}
