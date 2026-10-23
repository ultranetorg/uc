import { storesKeys } from "../stores/storesKeys"

export const storeFilesKeys = {
  all: (storeId: string) => [...storesKeys.detail(storeId), "files"] as const,
  paged: (storeId: string, page?: number, pageSize?: number) =>
    [...storeFilesKeys.all(storeId), { page, pageSize }] as const,
  infinite: (storeId: string, page?: number, pageSize?: number) =>
    [...storeFilesKeys.all(storeId), "infinite", { page, pageSize }] as const,
}
