export const proposalsKeys = {
  publications: (storeId: string) => ["moderator", "stores", storeId, "publications"] as const,
  moderators: (storeId: string) => ["stores", storeId, "proposals", "moderator"] as const,
  publishers: (storeId: string) => ["stores", storeId, "proposals", "publisher"] as const,
  userUnregistrations: (storeId: string) => ["stores", storeId, "proposals", "user-unregistrations"] as const,
}
