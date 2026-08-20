import { getFairApi } from "api"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useNextPaginationQuery } from "hooks"
import { ProductSearchResult, ProductType } from "types"

const api = getFairApi()

export const useSearchPaginatedProducts = (query?: string, productType?: ProductType) => {
  return useNextPaginationQuery<ProductSearchResult>({
    queryKey: ["products", { query, productType }],
    queryFn: (page, size) => api.searchProducts(query, productType, page, size),
    pageSize: DEFAULT_PAGE_SIZE_20,
    enabled: !!query,
  })
}
