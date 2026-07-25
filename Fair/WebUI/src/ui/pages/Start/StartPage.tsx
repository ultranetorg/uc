import { KeyboardEvent, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"
import { useDebounceValue } from "usehooks-ts"

import { SEARCH_DELAY } from "config"
import { useSearchLiteProducts, useSearchPaginatedProducts } from "entities"
import { useStoreTitle, useUrlParamsState } from "hooks"
import { NextPagination, MultilineText, SearchDropdown, SearchDropdownItem } from "ui/components"
import { ProductsGrid, ProductsGridEmpty, ProductsGridItem } from "ui/components/specific"
import { routes } from "utils"

export const StartPage = () => {
  const { t } = useTranslation("startPage")
  const navigate = useNavigate()

  useStoreTitle()

  const [state, setState] = useUrlParamsState({
    query: {
      defaultValue: "",
      validate: v => v !== "",
    },
  })

  const [query, setQuery] = useState(state.query)
  const [liteQuery, setLiteQuery] = useState("")
  const [debouncedLiteQuery] = useDebounceValue(liteQuery, SEARCH_DELAY)

  const { data: search, isPending: isSearchPending } = useSearchLiteProducts(debouncedLiteQuery)
  const searchItems = useMemo(() => search?.map(x => ({ value: x.publicationId, label: x.productTitle })), [search])

  const {
    data: products,
    page: paginationPage,
    loadedPagesCount,
    hasNext,
    isFetchingNext,
    onPageChange: onPaginationPageChange,
  } = useSearchPaginatedProducts(query)
  const productsItems = useMemo<ProductsGridItem[]>(
    () =>
      products.map<ProductsGridItem>(x => ({
        publicationId: x.publicationId,
        productTitle: x.productTitle,
        authorTitle: x.authorTitle,
        avatarId: x.avatarId,
      })),
    [products],
  )

  const handleChange = useCallback(
    (item?: SearchDropdownItem) => {
      if (item) {
        navigate(routes.publication("", item.value))
      }
    },
    [navigate],
  )

  const handleClearInputClick = useCallback(() => {
    setLiteQuery("")
    setQuery("")
    setState()
  }, [setState])

  const handleInputChange = useCallback((value: string) => setLiteQuery(value), [])

  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (e.key === "Enter") {
        setQuery(liteQuery)
        setState({ query: liteQuery })
      }
    },
    [liteQuery, setState],
  )

  const handleSearchClick = useCallback(() => {
    setQuery(liteQuery)
    setState({ query: liteQuery })
  }, [liteQuery, setState])

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ query: query })
      onPaginationPageChange(page)
    },
    [query, setState, onPaginationPageChange],
  )

  return (
    <div className="flex flex-col items-center gap-12 py-8">
      <div className="flex flex-col gap-4 text-center">
        <h1>
          <MultilineText>{t("title")}</MultilineText>
        </h1>
        <h5>
          <MultilineText>{t("description")}</MultilineText>
        </h5>
      </div>
      <SearchDropdown
        className="w-full max-w-[820px]"
        isLoading={isSearchPending}
        inputValue={state.query}
        items={searchItems}
        noticeMessage={t("notice")}
        placeholder={t("placeholder")}
        onChange={handleChange}
        onClearInputClick={handleClearInputClick}
        onInputChange={handleInputChange}
        onKeyDown={handleKeyDown}
        onSearchClick={handleSearchClick}
      />
      <ProductsGrid items={productsItems} />
      {query && (
        <>
          {loadedPagesCount > 0 ? (
            <NextPagination
              hasNext={hasNext && !isFetchingNext}
              page={paginationPage}
              loadedPages={loadedPagesCount}
              onPageChange={handlePageChange}
            />
          ) : (
            <ProductsGridEmpty message={t("noResults")} />
          )}
        </>
      )}
    </div>
  )
}
