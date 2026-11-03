import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { storeProposalsKeys } from "./storeProposalsKeys"

const api = getFairApi()

export const useGetStoreRenamingProposals = (storeId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => api.getStoreRenamingProposals(storeId!, page, pageSize, search)

  const { isPending, isError, data } = useQuery({
    queryKey: [...storeProposalsKeys.renamings(storeId!), { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isPending, isError, data }
}
