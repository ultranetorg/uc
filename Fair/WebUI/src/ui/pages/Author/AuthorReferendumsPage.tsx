import { useCallback, useEffect, useState } from "react"
import { Link, useParams, useSearchParams } from "react-router-dom"

import { DEFAULT_PAGE_SIZE, PAGE_SIZES } from "constants"
import { useGetAuthorReferendums } from "entities"
import { useQueryParams } from "hooks"
import { Input, Pagination, Select, SelectItem } from "ui/components"

const pageSizes: SelectItem[] = PAGE_SIZES.map(x => ({ label: x.toString(), value: x.toString() }))

export const AuthorReferendumsPage = () => {
  const [page, setPage] = useState(0)
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE)
  const [search, setSearch] = useState("")

  const { siteId } = useParams()
  const { isPending, data: referendums } = useGetAuthorReferendums(siteId, page, pageSize, search)

  const { page: queryPage, pageSize: querySize, search: querySearch } = useQueryParams()
  const [searchParams, setSearchParams] = useSearchParams()

  const pagesCount =
    referendums?.totalItems && referendums.totalItems > 0 ? Math.ceil(referendums.totalItems / pageSize) : 0

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
    <div className="my-3 flex flex-col gap-3">
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
            {referendums?.items?.map(r => (
              <tr key={r.id}>
                <td>
                  <Link to={`/${siteId}/a-r/${r.id}`}>{r.id}</Link>
                </td>
                <td>{r.flags}</td>
                <td>
                  <div>
                    <strong>Change:</strong> {r.proposal?.change}
                  </div>
                  <div>
                    <strong>Text:</strong> {r.proposal?.text}
                  </div>
                  <div>
                    <strong>First:</strong> {JSON.stringify(r.proposal?.first)}
                  </div>
                  <div>
                    <strong>Second:</strong> {JSON.stringify(r.proposal?.second)}
                  </div>
                </td>
                <td>{r.pros.length}</td>
                <td>{r.cons.length}</td>
                <td>{r.expiration}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
