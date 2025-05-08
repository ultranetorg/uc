import { KeyboardEvent, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"
import { useDebounceValue } from "usehooks-ts"

import { SEARCH_DELAY } from "constants"
import { useGetDefaultSites, useSearchLiteSites, useSearchSites } from "entities"
import { SearchDropdown, SearchDropdownItem, SitesList } from "ui/components"

import { PageHeader } from "./PageHeader"

export const SitesPage = () => {
  const { t } = useTranslation("sites")
  const navigate = useNavigate()

  const [query, setQuery] = useState("")
  const [liteQuery, setLiteQuery] = useState("")
  const [debouncedLiteQuery] = useDebounceValue(liteQuery, SEARCH_DELAY)

  const { data: defaultSites, isFetching: isDefaultSitesFetching } = useGetDefaultSites(!query)

  const { data: liteSites, isFetching: isLiteFetching } = useSearchLiteSites(debouncedLiteQuery)
  const liteItems = useMemo(() => liteSites?.map(x => ({ value: x.id, label: x.title })), [liteSites])

  const { isFetching, data: sites, error } = useSearchSites(query, 0)

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
  }, [])

  const handleInputChange = useCallback((value: string) => setLiteQuery(value), [])

  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (e.key === "Enter") {
        setQuery(liteQuery)
      }
    },
    [liteQuery],
  )

  const handleSearchClick = useCallback(() => {
    setQuery(liteQuery)
  }, [liteQuery])

  return (
    <div className="flex flex-col items-center gap-6 py-16">
      <PageHeader title={t("title")} description={t("description")} />
      <div className="w-full max-w-[820px]">
        <div className="flex w-full">
          <SearchDropdown
            className="w-full"
            isLoading={isLiteFetching}
            items={liteItems}
            onChange={handleChange}
            onClearInputClick={handleClearInputClick}
            onInputChange={handleInputChange}
            onKeyDown={handleKeyDown}
            onSearchClick={handleSearchClick}
          />
        </div>
      </div>
      <SitesList
        isFetching={isFetching ?? isDefaultSitesFetching}
        items={sites?.items ?? defaultSites}
        itemsCount={sites?.items.length ?? defaultSites?.length ?? 0}
        error={error}
      />
    </div>
  )
}
