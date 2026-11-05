import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { categoryProposalsKeys } from "./categoryProposalsKeys"

const api = getFairApi()

export const useGetCategoryMovementProposals = (
  storeId?: string,
  page?: number,
  pageSize?: number,
  search?: string,
) => {
  const queryFn = () => api.getCategoryMovementProposals(storeId!, page, pageSize, search)

  const { isLoading, isError, data } = useQuery({
    queryKey: [...categoryProposalsKeys.movements(storeId!), { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isLoading, isError, data }
}
