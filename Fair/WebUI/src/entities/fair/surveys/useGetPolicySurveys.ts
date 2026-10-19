import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { surveysKeys } from "./surveysKeys"

const api = getFairApi()

export const useGetPolicySurveys = (storeId?: string) => {
  const queryFn = () => api.getPolicySurveys(storeId!)

  const { isFetching, isError, data } = useQuery({
    queryKey: surveysKeys.policy(storeId!),
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isFetching, isError, data }
}
