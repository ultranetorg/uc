import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { surveyKeys } from "./surveyKeys"

const api = getFairApi()

export const useGetPolicySurveys = (storeId?: string) => {
  const queryFn = () => api.getPolicySurveys(storeId!)

  const { isFetching, isError, data } = useQuery({
    queryKey: surveyKeys.policies(storeId!),
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isFetching, isError, data }
}
