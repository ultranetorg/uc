import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { userProposalsKeys } from "./userProposalsKeys"

const api = getFairApi()

export const useGetUserUnregistrationProposals = (storeId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getUserUnregistrationProposals(storeId!, page, pageSize)

  const { isLoading, isError, data, refetch } = useQuery({
    queryKey: [...userProposalsKeys.unregistrations(storeId!), { page, pageSize }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isLoading, isError, data, refetch }
}
