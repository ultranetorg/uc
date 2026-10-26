import { moderationKeys } from "../moderationKeys"

export const reviewProposalsKeys = {
  all: (storeId: string) => [...moderationKeys.root(storeId), "reviews"] as const,

  creations: (storeId: string) => [...reviewProposalsKeys.all(storeId), "creations"] as const,
  deletions: (storeId: string) => [...reviewProposalsKeys.all(storeId), "deletions"] as const,
}
