import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetProductPublications = (productId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getProductPublications(productId!, page, pageSize)

  const { isLoading, isFetching, isError, data } = useQuery({
    queryKey: ["product", productId, "publications", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!productId,
  })

  return { isLoading, isFetching, isError, data }
}
