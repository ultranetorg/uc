export const usersKeys = {
  all: ["users"] as const,
  info: (userId: string) => [...usersKeys.all, userId] as const,
  detail: (userId: string) => [...usersKeys.info(userId), "details"] as const,
  site: (userId: string, siteId: string) => [...usersKeys.info(userId), "site", siteId] as const,
}
