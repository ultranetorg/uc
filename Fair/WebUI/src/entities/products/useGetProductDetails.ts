import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetProductDetails = (productId?: string) => {
  const queryFn = () => api.getProductDetails(productId!)

  const { isPending, isFetching, isError, data } = useQuery({
    queryKey: ["product", productId],
    queryFn: queryFn,
    enabled: !!productId,
  })

  return { isPending, isFetching, isError, data }
}
