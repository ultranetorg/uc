import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { storeProposalsKeys } from "./storeProposalsKeys"

const api = getFairApi()

export const useGetStoreInfoUpdationProposals = (
  storeId?: string,
  page?: number,
  pageSize?: number,
  search?: string,
) => {
  const queryFn = () => api.getStoreInfoUpdationProposals(storeId!, page, pageSize, search)

  const { isPending, isError, data } = useQuery({
    queryKey: [...storeProposalsKeys.infoUpdations(storeId!), { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isPending, isError, data }
}
