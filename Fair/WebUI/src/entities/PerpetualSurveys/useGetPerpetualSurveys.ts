import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetPerpetualSurveys = (siteId?: string) => {
  const queryFn = () => api.getAuthorPerpetualSurveys(siteId!)

  const { isFetching, isError, data } = useQuery({
    queryKey: ["sites", siteId, "perpetual-surveys"],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isFetching, isError, data }
}
