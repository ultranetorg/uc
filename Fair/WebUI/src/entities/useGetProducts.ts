import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetProducts = (name?: string, page?: number, pageSize?: number) => {
  const queryFn = () => {
    return api.getProducts(name, page, pageSize)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["products", { name, page, pageSize }],
    queryFn: queryFn,
  })

  return { isPending, error, data }
}
