import { useCallback, useEffect, useState } from "react"
import { Link, useParams, useSearchParams } from "react-router-dom"

import { DEFAULT_PAGE_SIZE, PAGE_SIZES } from "constants"
import { useGetModeratorDisputes } from "entities"
import { useQueryParams } from "hooks"
import { Input, Pagination, Select, SelectItem } from "ui/components"

const pageSizes: SelectItem[] = PAGE_SIZES.map(x => ({ label: x.toString(), value: x.toString() }))

export const ModeratorDisputesPage = () => {
  const [page, setPage] = useState(0)
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE)
  const [search, setSearch] = useState("")

  const { siteId } = useParams()
  const { isPending, data: disputes } = useGetModeratorDisputes(siteId, page, pageSize, search)

  const { page: queryPage, pageSize: querySize, search: querySearch } = useQueryParams()
  const [searchParams, setSearchParams] = useSearchParams()

  const pagesCount = disputes?.totalItems && disputes.totalItems > 0 ? Math.ceil(disputes.totalItems / pageSize) : 0

  useEffect(() => {
    if (page != queryPage) {
      setPage(queryPage)
    }
    if (pageSize != querySize) {
      setPageSize(querySize)
    }
    if (search != querySearch) {
      setSearch(querySearch)
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
    if (search !== "") {
      searchParams.set("search", search)
    } else {
      searchParams.delete("search")
    }
    setSearchParams(searchParams)
  }, [page, pageSize, searchParams, search, setSearchParams])

  const handlePageSizeChange = useCallback((value: string) => {
    setPage(0)
    setPageSize(parseInt(value))
  }, [])

  return (
    <div>
      <div className="flex w-80 gap-3">
        <Input placeholder="Search site" value={search} onChange={setSearch} />
        <Select items={pageSizes} value={pageSize} onChange={handlePageSizeChange} />
        <Pagination pagesCount={pagesCount} onClick={setPage} page={page} />
      </div>
      <div>
        {" "}
        <table style={{ width: "100%", borderCollapse: "collapse" }}>
          <thead>
            <tr>
              <th>ID</th>
              <th>Flags</th>
              <th>Proposals</th>
              <th>Props</th>
              <th>Cons</th>
              <th>Expiration</th>
            </tr>
          </thead>
          <tbody>
            {disputes?.items?.map(dispute => (
              <tr key={dispute.id}>
                <td>
                  <Link to={`/${siteId}/m-d/${dispute.id}`}>{dispute.id}</Link>
                </td>
                <td>{dispute.flags}</td>
                <td>
                  <div>
                    <strong>Change:</strong> {dispute.proposal?.change}
                  </div>
                  <div>
                    <strong>Text:</strong> {dispute.proposal?.text}
                  </div>
                  <div>
                    <strong>First:</strong> {JSON.stringify(dispute.proposal?.first)}
                  </div>
                  <div>
                    <strong>Second:</strong> {JSON.stringify(dispute.proposal?.second)}
                  </div>
                </td>
                <td>{dispute.pros.length}</td>
                <td>{dispute.cons.length}</td>
                <td>{dispute.expiration}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
