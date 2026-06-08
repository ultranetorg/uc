export const proposalsKeys = {
  moderators: (siteId: string) => ["sites", siteId, "proposals", "moderator"] as const,
  publishers: (siteId: string) => ["sites", siteId, "proposals", "publisher"] as const,
}
