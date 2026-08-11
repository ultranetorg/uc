import { useQuery } from "@tanstack/react-query"

import { getLocalFairApi } from "api"

const api = getLocalFairApi()

export const useGetVaultUrl = () => {
  const queryFn = () => api.getVaultUrl()

  const { isLoading, error, data } = useQuery({
    queryKey: ["urls", "vault"],
    queryFn: queryFn,
  })

  return { data, isLoading, error: error ?? undefined }
}
