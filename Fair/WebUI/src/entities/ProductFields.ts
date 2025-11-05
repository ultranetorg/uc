import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetProductFields = (productId: string) => {
  const queryFn = () => api.getProductFields(productId)

  return useQuery({
    queryKey: ["products", productId, "fields"],
    queryFn: queryFn,
  })
}

export const useGetProductCompareFields = (publicationId: string) => {
  const queryFn = () => api.getProductCompareFields(publicationId)

  return useQuery({
    queryKey: ["publications", publicationId, "updated-fields"],
    queryFn: queryFn,
  })
}
