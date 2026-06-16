import { useQuery } from "@tanstack/react-query"

import { getLocalFairApi } from "api"

const api = getLocalFairApi()

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
