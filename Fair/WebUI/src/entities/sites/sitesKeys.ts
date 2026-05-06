export const sitesKeys = {
  all: ["sites"] as const,
  default: () => [...sitesKeys.all, "default"],
  detail: (siteId: string) => [...sitesKeys.all, siteId] as const,
}
