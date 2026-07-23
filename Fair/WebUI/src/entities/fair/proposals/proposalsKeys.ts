export const proposalsKeys = {
  moderators: (storeId: string) => ["stores", storeId, "proposals", "moderator"] as const,
  publishers: (storeId: string) => ["stores", storeId, "proposals", "publisher"] as const,
}
