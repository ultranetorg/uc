export const proposalsKeys = {
  proposals: (storeId: string) => ["moderator", "stores", storeId, "discussions"] as const,
  proposalDetails: (storeId: string, proposalId: string) => [...proposalsKeys.proposals(storeId), proposalId] as const,

  publications: (storeId: string) => ["moderator", "stores", storeId, "publications"] as const,
  moderators: (storeId: string) => ["stores", storeId, "proposals", "moderator"] as const,
  publishers: (storeId: string) => ["stores", storeId, "proposals", "publisher"] as const,
  userUnregistrations: (storeId: string) => ["stores", storeId, "proposals", "user-unregistrations"] as const,
}
