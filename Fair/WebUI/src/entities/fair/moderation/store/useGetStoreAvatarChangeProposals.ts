import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { storeProposalsKeys } from "./storeProposalsKeys"

const api = getFairApi()

export const useGetStoreAvatarChangeProposals = (
  storeId?: string,
  page?: number,
  pageSize?: number,
  search?: string,
) => {
  const queryFn = () => api.getStoreAvatarChangeProposals(storeId!, page, pageSize, search)

  const { isLoading, isError, data } = useQuery({
    queryKey: [...storeProposalsKeys.avatarChanges(storeId!), { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isLoading, isError, data }
}
