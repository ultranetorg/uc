import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { surveysKeys } from "./surveysKeys"

const api = getFairApi()

export const useGetPolicySurveyDetails = (storeId?: string, surveyId?: string) => {
  const queryFn = () => api.getPolicySurveyDetails(storeId!, surveyId!)

  const { isFetching, isError, data, error, refetch } = useQuery({
    queryKey: surveysKeys.details(storeId!, surveyId!),
    queryFn: queryFn,
    enabled: !!storeId && surveyId !== undefined,
  })

  return { isFetching, isError, data, refetch, error: error ?? undefined }
}
