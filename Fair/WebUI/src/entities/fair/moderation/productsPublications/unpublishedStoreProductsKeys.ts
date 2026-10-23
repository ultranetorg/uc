import { moderationKeys } from "../moderationKeys"

export const unpublishedStoreProductsKeys = {
  all: (storeId: string) => [...moderationKeys.root(storeId), "products", "unpublished"] as const,
  detail: (storeId: string, productId: string) => [...unpublishedStoreProductsKeys.all(storeId), productId] as const,
}
