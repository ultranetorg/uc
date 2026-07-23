import { KeyboardEvent, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"
import { useDebounceValue } from "usehooks-ts"
import { isNumber } from "lodash"

import { useUserContext } from "app"
import { DEFAULT_PAGE_SIZE, SEARCH_DELAY } from "config"
import { useGetDefaultStores, useSearchLiteStores, useSearchStores } from "entities"
import { useStoreTitle, useUrlParamsState } from "hooks"
import { Pagination, SearchDropdown, SearchDropdownItem } from "ui/components"
import { StoresGrid, StoresGridEmpty } from "ui/components/specific"
import { parseInteger, routes } from "utils"

import { PageHeader } from "./PageHeader"

export const StoresPage = () => {
  const { t } = useTranslation("storesPage")
  const navigate = useNavigate()
  const { user } = useUserContext()

  useStoreTitle()

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
    query: {
      defaultValue: "",
      validate: v => v !== "",
    },
  })

  const [page, setPage] = useState(state.page)

  const [query, setQuery] = useState(state.query)
  const [liteQuery, setLiteQuery] = useState("")
  const [debouncedLiteQuery] = useDebounceValue(liteQuery, SEARCH_DELAY)

  const { data: defaultStores, isFetching: isDefaultStoresFetching } = useGetDefaultStores(!query)
  const { data: liteStores, isFetching: isLiteFetching } = useSearchLiteStores(debouncedLiteQuery)
  const liteItems = useMemo(() => liteStores?.map(x => ({ value: x.id, label: x.title })), [liteStores])
  const { isFetching, data: stores } = useSearchStores(query, page)
  const pagesCount = stores?.totalItems && stores.totalItems > 0 ? Math.ceil(stores.totalItems / DEFAULT_PAGE_SIZE) : 0

  const isDefaultFetching = !query && (!defaultStores || isDefaultStoresFetching)
  const isStoresFetching = query && (!stores || isFetching)

  const handleChange = useCallback(
    (item?: SearchDropdownItem) => {
      if (item) {
        navigate(routes.store(item.value))
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
        setState({ query: liteQuery, page: 0 })
        setPage(0)
      }
    },
    [liteQuery, setState],
  )

  const handleSearchClick = useCallback(() => {
    setQuery(liteQuery)
    setState({ query: liteQuery, page: 0 })
    setPage(0)
  }, [liteQuery, setState])

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ query: query, page: page })
      setPage(page)
    },
    [query, setState],
  )

  if (isDefaultFetching || isStoresFetching) {
    return <div>Loading</div>
  }

  return (
    <div className="flex flex-col items-center gap-12 py-8">
      <PageHeader title={t("title")} description={t("description")} />
      <div className="w-full max-w-[820px]">
        <SearchDropdown
          className="w-full"
          isLoading={isLiteFetching}
          inputValue={state.query}
          items={liteItems}
          noticeMessage={t("notice")}
          placeholder={t("placeholder")}
          onChange={handleChange}
          onClearInputClick={handleClearInputClick}
          onInputChange={handleInputChange}
          onKeyDown={handleKeyDown}
          onSearchClick={handleSearchClick}
        />
      </div>
      {(stores && stores!.items.length > 0) || (defaultStores && defaultStores!.length > 0) ? (
        <>
          <StoresGrid items={(stores?.items ?? defaultStores)!} showStar={!!user} />
          {stores && <Pagination page={page} pagesCount={pagesCount} onPageChange={handlePageChange} />}
        </>
      ) : (
        <StoresGridEmpty message={t("noResults")} />
      )}
    </div>
  )
}
