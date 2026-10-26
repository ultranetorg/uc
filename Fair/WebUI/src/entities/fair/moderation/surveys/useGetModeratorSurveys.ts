import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { surveyKeys } from "./surveyKeys"

const api = getFairApi()

export const useGetModeratorSurveys = (storeId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getModeratorSurveys(storeId!, page, pageSize)

  const { isFetching, isError, data, refetch } = useQuery({
    queryKey: [...surveyKeys.moderators(storeId!), { page, pageSize }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isFetching, isError, data, refetch }
}
