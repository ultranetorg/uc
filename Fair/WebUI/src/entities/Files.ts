import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetFile = (id?: string) => {
  const queryFn = () => api.getFile(id!)

  const { isLoading, isFetching, error, data } = useQuery({
    queryKey: ["files", id],
    queryFn: queryFn,
    enabled: !!id,
  })

  const isPending = !!id && (isLoading || isFetching)

  return { isPending, error: error ?? undefined, data }
}
