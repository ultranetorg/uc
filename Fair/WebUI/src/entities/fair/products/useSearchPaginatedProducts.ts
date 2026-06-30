import { getFairApi } from "api"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useNextPaginationQuery } from "hooks"
import { ProductSearchResult } from "types"

const api = getFairApi()

export const useSearchPaginatedProducts = (query?: string) => {
  return useNextPaginationQuery<ProductSearchResult>({
    queryKey: ["products", { query }],
    queryFn: (page, size) => api.searchProducts(query, page, size),
    pageSize: DEFAULT_PAGE_SIZE_20,
    enabled: !!query,
  })
}
