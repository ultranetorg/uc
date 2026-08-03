import { KeyboardEvent, useCallback, useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"
import { useDebounceValue } from "usehooks-ts"

import { SEARCH_DELAY } from "config"
import { useGetProductPublications, useSearchLiteProducts, useSearchPaginatedProducts } from "entities"
import { useStoreTitle, useUrlParamsState } from "hooks"
import { ProductPublication } from "types"
import { NextPagination, MultilineText, SearchDropdown, SearchDropdownItem } from "ui/components"
import { ProductsGrid, ProductsGridEmpty, ProductsGridItem } from "ui/components/specific"
import { routes } from "utils"

import { CategoryDropdown } from "./CategoryDropdown"

const PUBLICATIONS_PAGE_SIZE = 10

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
  const [clickedProductId, setClickedProductId] = useState<string | undefined>()
  const [additionalPublications, setAdditionalPublications] = useState<Record<string, ProductPublication[]>>({})

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
      products.map<ProductsGridItem>(x => {
        const additional = additionalPublications[x.productId] ?? []

        return {
          productId: x.productId,
          publicationId: x.publicationId,
          productTitle: x.productTitle,
          authorTitle: x.authorTitle,
          avatarId: x.avatarId,
          storesRatings: [...x.publications, ...additional],
          hasMorePublications: additional.length > 0 ? false : x.hasMorePublications,
        }
      }),
    [products, additionalPublications],
  )
  const { data: productPublications } = useGetProductPublications(clickedProductId, 1, PUBLICATIONS_PAGE_SIZE)

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

  const handleShowAllClick = useCallback((productId: string) => setClickedProductId(productId), [])

  useEffect(() => {
    if (!clickedProductId || !productPublications) return

    setAdditionalPublications(prev => ({ ...prev, [clickedProductId]: productPublications.items }))
  }, [clickedProductId, productPublications])

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
      <div className="flex w-full max-w-[780px] items-center gap-6">
        {/* <CategoryDropdown
          items={[
            { label: "Products", value: "products" },
            { label: "Stores", value: "stores" },
          ]}
          defaultValue="products"
          className="w-25 shrink-0"
        /> */}
        <SearchDropdown
          className="flex-1"
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
        {/* <ButtonPrimary label="Search" className="h-13 w-36" /> */}
      </div>
      <ProductsGrid items={productsItems} onShowAllClick={handleShowAllClick} />
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
