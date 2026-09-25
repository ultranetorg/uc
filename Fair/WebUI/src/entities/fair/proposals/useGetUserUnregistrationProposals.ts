import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { proposalsKeys } from "./proposalsKeys"

const api = getFairApi()

export const useGetUserUnregistrationProposals = (storeId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getUserUnregistrationProposals(storeId!, page, pageSize)

  const { isPending, isError, data, refetch } = useQuery({
    queryKey: [...proposalsKeys.userUnregistrations(storeId!), { page, pageSize }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isPending, isError, data, refetch }
}
