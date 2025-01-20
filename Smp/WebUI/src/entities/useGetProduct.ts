import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetProduct = (id?: string) => {
  const queryFn = () => {
    if (!id) {
      return
    }

    return api.getProduct(id)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["products", id],
    queryFn: queryFn,
    enabled: !!id,
  })

  return { isPending, error, data }
}
