import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { publisherProposalsKeys } from "./publisherProposalsKeys"

const api = getFairApi()

export const useGetPublisherProposals = (storeId?: string, search?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getPublisherProposals(storeId!, search, page, pageSize)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: [...publisherProposalsKeys.all(storeId!), { search, page, pageSize }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
