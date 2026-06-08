import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetModeratorUser = (name?: string) => {
  const queryFn = () => api.getModeratorUser(name!)

  const { isPending, error, data, isFetching, refetch } = useQuery({
    queryKey: ["moderator", "users", name],
    queryFn: queryFn,
    enabled: !!name,
  })

  return { isPending, error: error ?? undefined, data, isFetching, refetch }
}
