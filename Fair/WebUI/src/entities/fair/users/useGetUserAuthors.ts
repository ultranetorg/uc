import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { usersKeys } from "./usersKeys"

const api = getFairApi()

export const useGetUserAuthors = (userId?: string) => {
  const queryFn = () => api.getUserAuthors(userId!)

  const { isPending, isError, error, data, refetch } = useQuery({
    queryKey: usersKeys.authors(userId!),
    queryFn: queryFn,
    enabled: !!userId,
  })

  return { isPending, isError, error: error ?? undefined, data, refetch }
}
