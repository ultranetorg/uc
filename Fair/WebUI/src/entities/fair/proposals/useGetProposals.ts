import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"
import { proposalsKeys } from "./proposalsKeys"

const api = getFairApi()

export const useGetProposals = (storeId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => api.getModeratorDiscussions(storeId!, page, pageSize, search)

  const { isPending, error, data } = useQuery({
    queryKey: [...proposalsKeys.proposals(storeId!), { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isPending, error: error ?? undefined, data }
}
