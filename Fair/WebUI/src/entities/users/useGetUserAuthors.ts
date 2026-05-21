import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

import { usersKeys } from "./usersKeys"

const api = getApi()

export const useGetUserAuthors = (userId?: string) => {
  const queryFn = () => api.getUserAuthors(userId!)

  const { isPending, isError, data, refetch } = useQuery({
    queryKey: usersKeys.authors(userId!),
    queryFn: queryFn,
    enabled: !!userId,
  })

  return { isPending, isError, data, refetch }
}
