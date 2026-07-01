import { getFairApi } from "api"
import { DEFAULT_PAGE_SIZE } from "config"
import { useNextPaginationQuery } from "hooks"
import { PublicationExtended } from "types"

const api = getFairApi()

export const useSearchPaginatedPublications = (siteId?: string, query?: string) => {
  return useNextPaginationQuery<PublicationExtended>({
    queryKey: ["sites", siteId, "publications", { query }],
    queryFn: page => api.searchPublications(siteId!, query, page),
    pageSize: DEFAULT_PAGE_SIZE,
    enabled: !!query,
  })
}
