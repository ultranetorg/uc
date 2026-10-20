export const moderationReviewKeys = {
  all: (storeId: string) => ["moderator", "stores", storeId, "reviews"] as const,

  creations: (storeId: string) => [...moderationReviewKeys.all(storeId), "creations"] as const,
  deletions: (storeId: string) => [...moderationReviewKeys.all(storeId), "deletions"] as const,
}
