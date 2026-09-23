import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { usersKeys } from "./usersKeys"

const api = getFairApi()

export const useGetUserReviews = (userId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getUserReviews(userId!, page, pageSize)

  const { isError, isFetching, isPending, data, error } = useQuery({
    queryKey: usersKeys.reviews(userId!, page, pageSize),
    queryFn: queryFn,
    enabled: !!userId,
  })

  return { isError, isFetching, isPending, data, error }
}
