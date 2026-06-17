import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetDefaultSites = (enabled?: boolean) => {
  const queryFn = () => api.getDefaultSites()

  const { isFetching, error, data } = useQuery({
    queryKey: ["sites", "default"],
    queryFn: queryFn,
    enabled: !!enabled,
  })

  return { isFetching, error: error ?? undefined, data }
}
