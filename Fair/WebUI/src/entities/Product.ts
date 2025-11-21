import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetProduct = (productId?: string) => {
  const queryFn = () => api.getProduct(productId!)

  const { isPending, isError, data } = useQuery({
    queryKey: ["products", productId],
    queryFn: queryFn,
    enabled: !!productId,
  })

  return { isPending, isError, data }
}
