import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetPerpetualSurveyDetails = (siteId?: string, perpetualSurveyId?: string) => {
  const queryFn = () => api.getAuthorPerpetualSurveyDetails(siteId!, perpetualSurveyId!)

  const { isFetching, isError, data } = useQuery({
    queryKey: ["sites", siteId, "perpetual-surveys", perpetualSurveyId],
    queryFn: queryFn,
    enabled: !!siteId && perpetualSurveyId !== undefined,
  })

  return { isFetching, isError, data }
}
