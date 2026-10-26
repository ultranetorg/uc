import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { surveyKeys } from "./surveyKeys"

const api = getFairApi()

export const useGetPolicySurveyDetails = (storeId?: string, surveyId?: string) => {
  const queryFn = () => api.getPolicySurveyDetails(storeId!, surveyId!)

  const { isFetching, isError, data, error, refetch } = useQuery({
    queryKey: surveyKeys.policyDetails(storeId!, surveyId!),
    queryFn: queryFn,
    enabled: !!storeId && surveyId !== undefined,
  })

  return { isFetching, isError, data, refetch, error: error ?? undefined }
}
