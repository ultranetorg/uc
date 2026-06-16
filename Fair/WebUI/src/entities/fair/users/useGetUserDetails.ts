import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { usersKeys } from "./usersKeys"

const api = getFairApi()

export const useGetUserDetails = (name?: string) => {
  const queryFn = () => api.getUserDetails(name!)

  const { isFetching, isError, data, refetch } = useQuery({
    queryKey: usersKeys.detail(name!),
    queryFn: queryFn,
    enabled: !!name,
  })

  return { isFetching, isError, data, refetch }
}
