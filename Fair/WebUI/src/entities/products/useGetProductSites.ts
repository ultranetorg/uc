import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetProductSites = (productId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getProductStores(productId!, page, pageSize)

  const { isPending, isFetching, isError, data } = useQuery({
    queryKey: ["product", productId, { page, pageSize }],
    queryFn: queryFn,
    enabled: !!productId,
  })

  return { isPending, isFetching, isError, data }
}
