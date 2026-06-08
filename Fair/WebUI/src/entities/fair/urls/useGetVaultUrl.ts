import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

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
