import { keepPreviousData, useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { usersKeys } from "./usersKeys"

const api = getFairApi()

export const useGetUserReviews = (userId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getUserReviews(userId!, page, pageSize)

  const { isError, isLoading, isPlaceholderData, data, error } = useQuery({
    queryKey: usersKeys.reviews(userId!, page, pageSize),
    queryFn: queryFn,
    enabled: !!userId,
    placeholderData: keepPreviousData,
  })

  return { isError, isLoading: isLoading || isPlaceholderData, data, error }
}
