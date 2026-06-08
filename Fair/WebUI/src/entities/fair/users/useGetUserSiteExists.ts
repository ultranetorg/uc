import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { usersKeys } from "./usersKeys"

const api = getFairApi()

export const useGetUserSiteExists = (userId?: string, siteId?: string) => {
  const queryFn = () => api.getUserSiteExists(userId!, siteId!)

  const { isFetching, isError, data, refetch } = useQuery({
    queryKey: usersKeys.site(userId!, siteId!),
    queryFn: queryFn,
    enabled: !!userId && !!siteId,
  })

  return { isFetching, isError, data, refetch }
}
