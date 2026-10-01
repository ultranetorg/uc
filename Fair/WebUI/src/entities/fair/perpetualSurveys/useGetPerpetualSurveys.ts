import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { perpetualSurveysKeys } from "./perpetualSurveysKeys"

const api = getFairApi()

export const useGetPerpetualSurveys = (storeId?: string) => {
  const queryFn = () => api.getAuthorPerpetualSurveys(storeId!)

  const { isFetching, isError, data } = useQuery({
    queryKey: perpetualSurveysKeys.all(storeId!),
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isFetching, isError, data }
}
