import { useCallback, useEffect, useState } from "react"
import { Link, useSearchParams } from "react-router-dom"

import { useGetSites } from "entities"
import { useQueryParams } from "hooks"
import { Input, Pagination, Select, SelectItem } from "ui/components"
import { DEFAULT_PAGE_SIZE, PAGE_SIZES } from "constants"

const pageSizes: SelectItem[] = PAGE_SIZES.map(x => ({ label: x.toString(), value: x.toString() }))

type SiteCardProps = {
  title: string
}

const SiteCard = ({ title }: SiteCardProps) => (
  <div className="flex h-24 w-48 items-center justify-center rounded-md bg-gray-400 hover:font-semibold">{title}</div>
)

export const SitesPage = () => {
  const [page, setPage] = useState(0)
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE)
  const [title, setTitle] = useState("")

  const { isPending, data: sites } = useGetSites(page, pageSize, title)

  const { page: queryPage, pageSize: querySize, title: queryTitle } = useQueryParams()
  const [searchParams, setSearchParams] = useSearchParams()

  const pagesCount = sites?.totalItems && sites.totalItems > 0 ? Math.ceil(sites.totalItems / pageSize) : 0

  useEffect(() => {
    if (page != queryPage) {
      setPage(queryPage)
    }
    if (pageSize != querySize) {
      setPageSize(querySize)
    }
    if (title != queryTitle) {
      setTitle(queryTitle)
    }
  }, [])

  useEffect(() => {
    if (!isPending && pagesCount > 0 && page > pagesCount) {
      setPage(0)
    }
  }, [isPending, page, pagesCount])

  useEffect(() => {
    if (page !== 0) {
      searchParams.set("page", page.toString())
    } else {
      searchParams.delete("page")
    }
    if (pageSize !== DEFAULT_PAGE_SIZE) {
      searchParams.set("pageSize", pageSize.toString())
    } else {
      searchParams.delete("pageSize")
    }
    if (title !== "") {
      searchParams.set("title", title)
    } else {
      searchParams.delete("title")
    }
    setSearchParams(searchParams)
  }, [page, pageSize, searchParams, title, setSearchParams])

  const handlePageSizeChange = useCallback((value: string) => {
    setPage(0)
    setPageSize(parseInt(value))
  }, [])

  return (
    <div className="flex flex-col">
      <div className="flex flex-col gap-3">
        <h1>
          <center>LIST OF ALL SITES</center>
        </h1>
        <div className="flex w-80 gap-3">
          <Input placeholder="Search site" value={title} onChange={setTitle} />
          <Select items={pageSizes} value={pageSize} onChange={handlePageSizeChange} />
          <Pagination pagesCount={pagesCount} onClick={setPage} page={page} />
        </div>
        {isPending || !sites ? (
          <div>LOADING</div>
        ) : !sites.items ? (
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
