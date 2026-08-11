import { getFairApi } from "api"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useNextPaginationQuery } from "hooks"
import { ProductType, PublicationExtended } from "types"

const api = getFairApi()

export const useSearchPaginatedPublications = (
  storeId?: string,
  query?: string,
  categoriesIds?: string[],
  type?: ProductType,
) => {
  return useNextPaginationQuery<PublicationExtended>({
    queryFn: page => api.searchPublications(storeId!, query, categoriesIds, type, page),
    queryKey: ["stores", storeId, "publications", { query, categoriesIds, type }],
    pageSize: DEFAULT_PAGE_SIZE_20,
    enabled: !!query,
  })
}
