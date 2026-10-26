import { moderationKeys } from "../moderationKeys"

export const changedPublicationsKeys = {
  all: (storeId: string) => [...moderationKeys.root(storeId), "publications", "changed"] as const,
  paged: (storeId: string, page?: number, pageSize?: number) =>
    [...changedPublicationsKeys.all(storeId), { page, pageSize }] as const,
  detail: (storeId: string, changedPublicationId: string) =>
    [...changedPublicationsKeys.all(storeId), changedPublicationId] as const,
}
