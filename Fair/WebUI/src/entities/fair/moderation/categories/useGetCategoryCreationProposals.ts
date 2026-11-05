import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { categoryProposalsKeys } from "./categoryProposalsKeys"

const api = getFairApi()

export const useGetCategoryCreationProposals = (
  storeId?: string,
  page?: number,
  pageSize?: number,
  search?: string,
) => {
  const queryFn = () => api.getCategoryCreationProposals(storeId!, page, pageSize, search)

  const { isLoading, isError, data } = useQuery({
    queryKey: [...categoryProposalsKeys.creations(storeId!), { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isLoading, isError, data }
}
