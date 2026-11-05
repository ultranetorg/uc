import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { publicationProposalsKeys } from "./publicationProposalsKeys"

const api = getFairApi()

export const useGetPublicationUnpublishProposals = (
  storeId?: string,
  page?: number,
  pageSize?: number,
  search?: string,
) => {
  const queryFn = () => api.getPublicationUnpublishProposals(storeId!, page, pageSize, search)

  const { isLoading, isError, data } = useQuery({
    queryKey: [...publicationProposalsKeys.unpublishes(storeId!), { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isLoading, isError, data }
}
