import { useCallback, useEffect, useState } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { useSearchLightSites, useSearchSites } from "entities"
import { DEFAULT_PAGE_SIZE } from "constants"
import { Pagination, SearchDropdown, SearchDropdownItem, SitesList } from "ui/components"
import { usePagePagination } from "ui/pages/hooks"

import { PageHeader } from "./PageHeader"

export const SitesPage = () => {
  const { t } = useTranslation("sites")
  const navigate = useNavigate()

  const [query, setQuery] = useState("")
  const [isDropdownHidden, setDropdownHidden] = useState(false)
  const { page, setPage, search, setSearch } = usePagePagination()
  const { data: options } = useSearchLightSites(query)

  const { isPending, data: sites, error } = useSearchSites(page, search)

  const pagesCount = sites?.totalItems && sites.totalItems > 0 ? Math.ceil(sites.totalItems / DEFAULT_PAGE_SIZE) : 0

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

  useEffect(() => {
    if (!isPending && pagesCount > 0 && page > pagesCount) {
      setPage(0)
    }
  }, [isPending, page, pagesCount, setPage])

  return (
    <div className="flex flex-col items-center gap-6 py-16">
      <PageHeader title={t("title")} description={t("description")} />
      <div className="w-full max-w-[820px]">
        <div className="flex h-12 items-center gap-3">
          {/* <Input placeholder="Search site" value={search} onChange={setSearch} /> */}
          <SearchDropdown
            className="flex-grow"
            isDropdownHidden={isDropdownHidden}
            items={options}
            value={query}
            onChange={setQuery}
            onKeyDown={handleKeyDown}
            onSelectItem={handleSelectItem}
          />
          <Pagination pagesCount={pagesCount} onClick={setPage} page={page} />
        </div>
      </div>
      <SitesList isPending={isPending} sites={sites} error={error} />
    </div>
  )
}
