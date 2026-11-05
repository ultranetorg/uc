import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { publicationProposalsKeys } from "./publicationProposalsKeys"

const api = getFairApi()

export const useGetPublicationCreationProposals = (
  storeId?: string,
  page?: number,
  pageSize?: number,
  search?: string,
) => {
  const queryFn = () => api.getPublicationCreationProposals(storeId!, page, pageSize, search)

  const { isLoading, isError, data } = useQuery({
    queryKey: [...publicationProposalsKeys.creations(storeId!), { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isLoading, isError, data }
}
