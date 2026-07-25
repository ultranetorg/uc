export const usersKeys = {
  all: ["users"] as const,
  info: (name: string) => [...usersKeys.all, name] as const,
  authors: (userId: string) => [...usersKeys.all, userId, "authors"] as const,
  detail: (name: string) => [...usersKeys.all, name, "details"] as const,
  reviews: (userId: string) => [...usersKeys.all, userId, "reviews"] as const,
  store: (userId: string, storeId: string) => [...usersKeys.all, userId, "store", storeId] as const,
}
