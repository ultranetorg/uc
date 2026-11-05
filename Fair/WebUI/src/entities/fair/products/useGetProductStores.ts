import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetProductStores = (productId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getProductStores(productId!, page, pageSize)

  const { isLoading, isFetching, isError, data } = useQuery({
    queryKey: ["product", productId, "stores", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!productId,
  })

  return { isLoading, isFetching, isError, data }
}
