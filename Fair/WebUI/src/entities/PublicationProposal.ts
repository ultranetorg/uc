import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetPublicationProposal = (siteId?: string, proposalId?: string) => {
  const queryFn = () => api.getPublicationProposal(proposalId!)

  const { isPending, error, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "publications", proposalId],
    queryFn: queryFn,
    enabled: !!siteId && !!proposalId,
  })

  return { isPending, error: error ?? undefined, data }
}

export const useGetPublicationProposals = (siteId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => api.getPublicationProposals(siteId!, page, pageSize, search)

  const { isPending, isError, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "publications", { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, isError, data }
}
