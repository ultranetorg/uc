import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetProductDetails = (productId?: string) => {
  const queryFn = () => api.getProductDetails(productId!)

  const { isPending, isFetching, isError, data } = useQuery({
    queryKey: ["product", productId],
    queryFn: queryFn,
    enabled: !!productId,
  })

  return { isPending, isFetching, isError, data }
}
