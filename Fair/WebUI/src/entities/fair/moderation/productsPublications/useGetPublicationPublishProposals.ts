import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { publicationProposalsKeys } from "./publicationProposalsKeys"

const api = getFairApi()

export const useGetPublicationPublishProposals = (
  storeId?: string,
  page?: number,
  pageSize?: number,
  search?: string,
) => {
  const queryFn = () => api.getPublicationPublishProposals(storeId!, page, pageSize, search)

  const { isPending, isError, data } = useQuery({
    queryKey: [...publicationProposalsKeys.publishes(storeId!), { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isPending, isError, data }
}
