import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetPerpetualSurveyComments = (
  siteId?: string,
  perpetualSurveyId?: string,
  page?: number,
  pageSize?: number,
) => {
  const queryFn = () => api.getAuthorPerpetualSurveyComments(siteId!, perpetualSurveyId!, page!, pageSize!)

  const { isFetching, isError, data } = useQuery({
    queryKey: ["sites", siteId, "perpetual-surveys", perpetualSurveyId, "comments", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId && perpetualSurveyId !== undefined && page !== undefined && pageSize !== undefined,
  })

  return { isFetching, isError, data }
}
