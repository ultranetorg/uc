import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetUserRegistrationProposals = (siteId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getUserRegistrationProposals(siteId!, page, pageSize)

  const { isPending, isError, data, refetch } = useQuery({
    queryKey: ["sites", siteId, "proposals", "user-registrations", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, isError, data, refetch }
}
