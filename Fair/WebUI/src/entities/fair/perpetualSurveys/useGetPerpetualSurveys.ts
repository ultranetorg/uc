import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetPerpetualSurveys = (siteId?: string) => {
  const queryFn = () => api.getAuthorPerpetualSurveys(siteId!)

  const { isFetching, isError, data } = useQuery({
    queryKey: ["sites", siteId, "perpetual-surveys"],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isFetching, isError, data }
}
