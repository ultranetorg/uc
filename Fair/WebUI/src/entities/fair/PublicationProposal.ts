import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetPublicationProposals = (storeId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => api.getPublicationProposals(storeId!, page, pageSize, search)

  const { isPending, isError, data } = useQuery({
    queryKey: ["moderator", "stores", storeId, "publications", { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isPending, isError, data }
}
