import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetUserDetails = (name?: string) => {
  const queryFn = () => api.getUserDetails(name!)

  const { isFetching, isError, data, refetch } = useQuery({
    queryKey: ["users", name, "details"],
    queryFn: queryFn,
    enabled: !!name,
  })

  return { isFetching, isError, data, refetch }
}
