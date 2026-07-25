export const storesKeys = {
  all: ["stores"] as const,
  default: () => [...storesKeys.all, "default"],
  detail: (storeId: string) => [...storesKeys.all, storeId] as const,

  users: (storeId: string) => [...storesKeys.detail(storeId), "users"] as const,
  publishers: (storeId: string) => [...storesKeys.detail(storeId), "publishers"] as const,
  moderators: (storeId: string) => [...storesKeys.detail(storeId), "moderators"] as const,
  policies: (storeId: string) => [...storesKeys.detail(storeId), "policies"] as const,
}
