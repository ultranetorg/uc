import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { proposalsKeys } from "./proposalsKeys"

const api = getFairApi()

export const useGetProposalDetails = (storeId?: string, proposalId?: string) => {
  const queryFn = () => api.getProposalDetails(storeId!, proposalId!)

  const { isFetching, error, data } = useQuery({
    queryKey: proposalsKeys.detail(storeId!, proposalId!),
    queryFn: queryFn,
    enabled: !!storeId && !!proposalId,
  })

  return { isFetching, error: error ?? undefined, data }
}
