import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { categoryProposalsKeys } from "./categoryProposalsKeys"

const api = getFairApi()

export const useGetCategoryDeletionProposals = (
  storeId?: string,
  page?: number,
  pageSize?: number,
  search?: string,
) => {
  const queryFn = () => api.getCategoryDeletionProposals(storeId!, page, pageSize, search)

  const { isPending, isError, data } = useQuery({
    queryKey: [...categoryProposalsKeys.deletions(storeId!), { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isPending, isError, data }
}
