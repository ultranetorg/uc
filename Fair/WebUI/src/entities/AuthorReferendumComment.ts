import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetAuthorReferendumComment = (
  siteId?: string,
  referendumId?: string,
  page?: number,
  pageSize?: number,
) => {
  const queryFn = () => api.getAuthorReferendumComments(siteId!, referendumId!, page, pageSize)

  const { isFetching, error, data } = useQuery({
    queryKey: ["author", "sites", siteId, "referendums", referendumId, "comments", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId && !!referendumId,
  })

  return { isFetching, error: error ?? undefined, data }
}
