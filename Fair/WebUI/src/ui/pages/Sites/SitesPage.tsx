import { useCallback, useEffect } from "react"
import { useTranslation } from "react-i18next"

import { useGetSites } from "entities"
import { PAGE_SIZES } from "constants"
import { Input, Pagination, Select, SelectItem, SitesList } from "ui/components"
import { usePagePagination } from "ui/pages/hooks"

import { PageHeader } from "./PageHeader"

const pageSizes: SelectItem[] = PAGE_SIZES.map(x => ({ label: x.toString(), value: x.toString() }))

export const SitesPage = () => {
  const { t } = useTranslation("sites")

  const { page, setPage, pageSize, setPageSize, search, setSearch } = usePagePagination()

  const { isPending, data: sites, error } = useGetSites(page, pageSize, search)

  const pagesCount = sites?.totalItems && sites.totalItems > 0 ? Math.ceil(sites.totalItems / pageSize) : 0

  useEffect(() => {
    if (!isPending && pagesCount > 0 && page > pagesCount) {
      setPage(0)
    }
  }, [isPending, page, pagesCount, setPage])

  const handlePageSizeChange = useCallback(
    (value: string) => {
      setPage(0)
      setPageSize(parseInt(value))
    },
    [setPage, setPageSize],
  )

  return (
    <div className="flex flex-col gap-6 py-16">
      <PageHeader title={t("title")} description={t("description")} />
      <div className="mx-auto flex h-12 items-center gap-3">
        <Input placeholder="Search site" value={search} onChange={setSearch} />
        <Select items={pageSizes} value={pageSize} onChange={handlePageSizeChange} />
        <Pagination pagesCount={pagesCount} onClick={setPage} page={page} />
      </div>
      <SitesList isPending={isPending} sites={sites} error={error} />
    </div>
  )
}
