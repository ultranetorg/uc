import { getFairApi } from "api"
import { DEFAULT_PAGE_SIZE } from "config"
import { useNextPaginationQuery } from "hooks"
import { PublicationExtended } from "types"

const api = getFairApi()

export const useSearchPaginatedPublications = (storeId?: string, query?: string) => {
  return useNextPaginationQuery<PublicationExtended>({
    queryKey: ["stores", storeId, "publications", { query }],
    queryFn: page => api.searchPublications(storeId!, query, page),
    pageSize: DEFAULT_PAGE_SIZE,
    enabled: !!query,
  })
}
