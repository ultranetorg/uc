import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetPerpetualSurveyDetails = (storeId?: string, perpetualSurveyId?: string) => {
  const queryFn = () => api.getAuthorPerpetualSurveyDetails(storeId!, perpetualSurveyId!)

  const { isFetching, isError, data, refetch } = useQuery({
    queryKey: ["stores", storeId, "perpetual-surveys", perpetualSurveyId],
    queryFn: queryFn,
    enabled: !!storeId && perpetualSurveyId !== undefined,
  })

  return { isFetching, isError, data, refetch }
}
