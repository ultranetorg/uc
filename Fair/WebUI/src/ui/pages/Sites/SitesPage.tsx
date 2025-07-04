import { KeyboardEvent, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"
import { useDebounceValue } from "usehooks-ts"
import { isNumber } from "lodash"

import { DEFAULT_PAGE_SIZE, SEARCH_DELAY } from "config"
import { useGetDefaultSites, useSearchLiteSites, useSearchSites } from "entities"
import { useUrlParamsState } from "hooks"
import { Pagination, SearchDropdown, SearchDropdownItem } from "ui/components"
import { SitesGrid, SitesGridEmpty } from "ui/components/specific"
import { parseInteger } from "utils"

import { PageHeader } from "./PageHeader"

export const SitesPage = () => {
  const { t } = useTranslation("sites")
  const navigate = useNavigate()

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

  const { data: defaultSites, isFetching: isDefaultSitesFetching } = useGetDefaultSites(!query)
  const { data: liteSites, isFetching: isLiteFetching } = useSearchLiteSites(debouncedLiteQuery)
  const liteItems = useMemo(() => liteSites?.map(x => ({ value: x.id, label: x.title })), [liteSites])
  const { isFetching, data: sites } = useSearchSites(query, page)
  const pagesCount = sites?.totalItems && sites.totalItems > 0 ? Math.ceil(sites.totalItems / DEFAULT_PAGE_SIZE) : 0

  const isDefaultFetching = !query && (!defaultSites || isDefaultSitesFetching)
  const isSitesFetching = query && (!sites || isFetching)

  const handleChange = useCallback(
    (item?: SearchDropdownItem) => {
      if (item) {
        navigate(`/${item.value}`)
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

  if (isDefaultFetching || isSitesFetching) {
    // TODO: add spinner or skeleton
    return <>🕑 LOADING</>
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
      {(sites && sites!.items.length > 0) || (defaultSites && defaultSites!.length > 0) ? (
        <>
          <SitesGrid items={(sites?.items ?? defaultSites)!} />
          {sites && <Pagination page={page} pagesCount={pagesCount} onPageChange={handlePageChange} />}
        </>
      ) : (
        <SitesGridEmpty message={t("noResults")} />
      )}
    </div>
  )
}
