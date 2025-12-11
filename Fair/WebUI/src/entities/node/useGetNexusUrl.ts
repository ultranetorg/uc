import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetNexusUrl = () => {
  const queryFn = () => api.getNexusUrl()

  const { isLoading, error, data } = useQuery({
    queryKey: ["urls", "nexus"],
    queryFn: queryFn,
    staleTime: Infinity,
    refetchOnWindowFocus: false,
  })

  return { data, isLoading, error: error ?? undefined }
}
