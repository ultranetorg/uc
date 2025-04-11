import { useCallback, useEffect } from "react"
import { Link } from "react-router-dom"

import { useGetSites } from "entities"
import { PAGE_SIZES } from "constants"
import { Input, Pagination, Select, SelectItem } from "ui/components"
import { usePagePagination } from "ui/pages/hooks"

const pageSizes: SelectItem[] = PAGE_SIZES.map(x => ({ label: x.toString(), value: x.toString() }))

type SiteCardProps = {
  title: string
  nickname?: string
}

const SiteCard = ({ title, nickname }: SiteCardProps) => (
  <div className="flex h-24 w-48 flex-col items-center justify-center rounded-md bg-gray-400 hover:font-semibold">
    <span>{title}</span>
    {nickname && <span className="text-sm text-gray-600">Nickname: {nickname}</span>}
  </div>
)

export const SitesPage = () => {
  const { page, setPage, pageSize, setPageSize, search, setSearch } = usePagePagination()

  const { isPending, data: sites } = useGetSites(page, pageSize, search)

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
    <div className="flex flex-col">
      <div className="flex flex-col gap-3">
        <h1>
          <center>LIST OF ALL SITES</center>
        </h1>
        <div className="flex w-80 gap-3">
          <Input placeholder="Search site" value={search} onChange={setSearch} />
          <Select items={pageSizes} value={pageSize} onChange={handlePageSizeChange} />
          <Pagination pagesCount={pagesCount} onClick={setPage} page={page} />
        </div>
        {isPending || !sites ? (
          <div>LOADING</div>
        ) : sites.items.length === 0 ? (
          <div>🚫 NO SITES</div>
        ) : (
          <div className="flex h-full w-full flex-wrap gap-3">
            {sites.items.map(x => (
              <Link key={x.id} to={`/${x.id}`}>
                <SiteCard title={x.title} />
              </Link>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
