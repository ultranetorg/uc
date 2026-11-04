export const usersKeys = {
  all: ["users"] as const,
  info: (name: string) => [...usersKeys.all, name] as const,
  authors: (userId: string) => [...usersKeys.all, userId, "authors"] as const,
  detail: (name: string) => [...usersKeys.all, name, "details"] as const,
  reviews: (userId: string, page?: number, pageSize?: number) =>
    [...usersKeys.all, userId, "reviews", { page, pageSize }] as const,
  // Членство пользователей в сторе. Префикс storeMembers позволяет инвалидировать членство всех пользователей стора сразу.
  storeMembers: (storeId: string) => [...usersKeys.all, "store", storeId] as const,
  store: (userId: string, storeId: string) => [...usersKeys.storeMembers(storeId), userId] as const,
}
