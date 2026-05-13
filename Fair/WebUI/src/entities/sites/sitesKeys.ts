export const sitesKeys = {
  all: ["sites"] as const,
  default: () => [...sitesKeys.all, "default"],
  detail: (siteId: string) => [...sitesKeys.all, siteId] as const,

  publishers: (siteId: string) => [...sitesKeys.detail(siteId), "publishers"] as const,
  moderators: (siteId: string) => [...sitesKeys.detail(siteId), "moderators"] as const,
}
