import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetVaultUrl = () => {
  const queryFn = () => api.getVaultUrl()

  const { isLoading, error, data } = useQuery({
    queryKey: ["urls", "vault"],
    queryFn: queryFn,
    staleTime: Infinity,
    refetchOnWindowFocus: false,
  })

  return { data, isLoading, error: error ?? undefined }
}
