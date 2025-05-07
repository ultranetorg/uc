import { useCallback, useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { useGetDefaultSites, useSearchLightSites, useSearchSites } from "entities"
import { SearchDropdown, SearchDropdownItem, SitesList } from "ui/components"
import { usePagePagination } from "ui/pages/hooks"

import { PageHeader } from "./PageHeader"

export const SitesPage = () => {
  const { t } = useTranslation("sites")
  const navigate = useNavigate()

  const [query, setQuery] = useState("")
  const [isDropdownHidden, setDropdownHidden] = useState(false)
  const { page, setPage, search, setSearch } = usePagePagination()

  const { data: defaultSites, isFetching: isDefaultSitesFetching } = useGetDefaultSites(!query)
  const { data: options } = useSearchLightSites(query)
  const { isFetching, data: sites, error } = useSearchSites(search, page)

  const pagesCount = 10

  const handleKeyDown = useCallback(
    (key: string) => {
      if (key === "Enter") {
        setSearch(query)
        setDropdownHidden(true)
      } else {
        setDropdownHidden(false)
      }
    },
    [query, setSearch],
  )

  const handleSelectItem = useCallback((e: SearchDropdownItem) => navigate(`/${e.id}`), [navigate])

  const items = useMemo<SearchDropdownItem[]>(() => options?.map(x => ({ id: x.id, value: x.title })) ?? [], [options])

  useEffect(() => {
    if (!isFetching && pagesCount > 0 && page > pagesCount) {
      setPage(0)
    }
  }, [isFetching, page, pagesCount, setPage])

  return (
    <div className="flex flex-col items-center gap-6 py-16">
      <PageHeader title={t("title")} description={t("description")} />
      <div className="w-full max-w-[820px]">
        <div className="flex h-12 items-center gap-3">
          <SearchDropdown
            className="flex-grow"
            isDropdownHidden={isDropdownHidden}
            items={items}
            value={query}
            onChange={setQuery}
            onKeyDown={handleKeyDown}
            onSelectItem={handleSelectItem}
          />
          {/* <Pagination pagesCount={pagesCount} onClick={setPage} page={page} /> */}
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
