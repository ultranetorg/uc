import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetPerpetualSurveys = (storeId?: string) => {
  const queryFn = () => api.getAuthorPerpetualSurveys(storeId!)

  const { isFetching, isError, data } = useQuery({
    queryKey: ["stores", storeId, "perpetual-surveys"],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isFetching, isError, data }
}
