import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { storesKeys } from "./storesKeys"

const api = getFairApi()

export const useGetStoreModerators = (storesId?: string) => {
  const queryFn = () => api.getStoreModerators(storesId!)

  const { isFetching, error, data, refetch } = useQuery({
    queryKey: storesKeys.moderators(storesId!),
    queryFn: queryFn,
    enabled: !!storesId,
  })

  return { isFetching, error: error ?? undefined, data, refetch }
}
