import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { unpublishedStoreProductsKeys } from "./unpublishedStoreProductsKeys"

const api = getFairApi()

export const useGetUnpublishedStoreProduct = (storeId?: string, unpublishedProductId?: string) => {
  const queryFn = () => api.getUnpublishedStoreProduct(storeId!, unpublishedProductId!)

  const { isLoading, isFetching, isError, data } = useQuery({
    queryKey: unpublishedStoreProductsKeys.detail(storeId!, unpublishedProductId!),
    queryFn: queryFn,
    enabled: !!storeId && !!unpublishedProductId,
  })

  return { isLoading, isFetching, isError, data }
}
