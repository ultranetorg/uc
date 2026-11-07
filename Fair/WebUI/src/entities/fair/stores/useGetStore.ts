import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { storesKeys } from "./storesKeys"

const api = getFairApi()

export const useGetStore = (storesId?: string) => {
  const queryFn = () => api.getStore(storesId!)

  const { isLoading, error, data } = useQuery({
    queryKey: storesKeys.detail(storesId!),
    queryFn: queryFn,
    enabled: !!storesId,
  })

  return { isLoading, error: error ?? undefined, data }
}
