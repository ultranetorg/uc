import { useCallback, useState } from "react"
import { useTranslation } from "react-i18next"

import { useSearchPaginatedProducts, useSearchStores } from "entities"
import { useStoreTitle, useUrlParamsState } from "hooks"
import { ProductType } from "types"
import { MessageBox, MultilineText, NextPagination } from "ui/components"
import { FiltersPanel, ProductsList, StoresList, SearchInput, SearchScope } from "ui/components/specific"

import { ParadigmDescription } from "./ParadigmDescription"

export const IndexPage = () => {
  const { t } = useTranslation("indexPage")

  const [state, setState] = useUrlParamsState({
    query: {
      defaultValue: "",
      validate: v => v !== "",
    },
    type: {
      defaultValue: "none",
      validate: v =>
        v === "stores" || v === "software" || v === "movie" || v === "music" || v === "book" || v === "game",
    },
  })

  const [searchValue, setSearchValue] = useState(state.query)
  const [search, setSearch] = useState(state.query)

  const [scope, setScope] = useState<SearchScope>(state.type === "stores" ? "stores" : "products")
  const [filter, setFilter] = useState<ProductType>(state.type === "stores" ? "none" : (state.type as ProductType))

  useStoreTitle()

  const {
    isPending: isProductsPending,
    data: products,
    page,
    loadedPagesCount,
    hasNext,
    isFetchingNext,
    onPageChange,
  } = useSearchPaginatedProducts(scope === "products" ? search : undefined, filter)
  const { isPending: isStoresPending, data: stores } = useSearchStores(scope === "stores" ? search : undefined)

  const handleClear = useCallback(() => {
    setState({ query: "", type: "none" })
    setScope("products")
    setFilter("none")
    setSearchValue("")
    setSearch("")
  }, [setState])

  const handleSearch = useCallback(() => {
    setSearch(searchValue)
    setState({ query: searchValue, type: scope === "stores" ? "stores" : filter })
  }, [filter, scope, searchValue, setState])

  const handleScopeChange = useCallback(
    (value: SearchScope) => {
      setScope(value)
      setState({ type: value === "stores" ? "stores" : filter })
    },
    [filter, setState],
  )

  const handleFilterChange = useCallback(
    (value: ProductType) => {
      setFilter(value)
      setState({ type: value })
    },
    [setState],
  )

  const handlePageChange = useCallback(
    (page: number) => {
      //setState({ query: searchQuery })
      onPageChange(page)
    },
    [onPageChange],
  )

  const searchMode = search !== ""

  if (searchMode && ((scope === "products" && isProductsPending) || (scope === "stores" && isStoresPending))) {
    return <div>Loading</div>
  }

  return (
    <div className="flex flex-col items-center gap-12 py-8">
      {!searchMode && (
        <div className="flex flex-col gap-4 text-center">
          <h1>
            <MultilineText>{t("title")}</MultilineText>
          </h1>
          <h5>
            <MultilineText>{t("description")}</MultilineText>
          </h5>
        </div>
      )}
      <div className="flex w-full max-w-[900px] flex-col items-center gap-6">
        <div className="flex w-full flex-col items-center gap-4">
          <SearchInput
            scope={scope}
            onScopeChange={handleScopeChange}
            onClear={handleClear}
            value={searchValue}
            onChange={setSearchValue}
            onSearch={handleSearch}
          />
          {searchMode && scope === "products" && <FiltersPanel value={filter} onChange={handleFilterChange} />}
        </div>
        {!searchMode ? (
          <ParadigmDescription />
        ) : (
          <div className="flex w-full flex-col gap-4">
            <span className="text-2base font-semibold leading-5">{t("searchResults")}</span>
            {(scope === "stores" ? (stores?.items.length ?? 0) : products.length) > 0 ? (
              <>
                {scope === "stores" ? <StoresList items={stores?.items ?? []} /> : <ProductsList items={products} />}
                <NextPagination
                  hasNext={hasNext && !isFetchingNext}
                  page={page}
                  loadedPages={loadedPagesCount}
                  onPageChange={handlePageChange}
                />
              </>
            ) : (
              <MessageBox className="p-6" message={t("noResults")} />
            )}
          </div>
        )}
      </div>
    </div>
  )
}
