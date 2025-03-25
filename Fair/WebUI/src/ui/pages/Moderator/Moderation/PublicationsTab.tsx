import { useCallback, useEffect, useState } from "react"
import { Link, useParams, useSearchParams } from "react-router-dom"

import { DEFAULT_PAGE_SIZE } from "constants"
import { useGetModeratorPublications } from "entities"
import { useQueryParams } from "hooks"
import { Input, Pagination, Select } from "ui/components"

export const PublicationsTab = () => {
  const [page, setPage] = useState(0)
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE)
  const [search, setSearch] = useState("")

  const { siteId } = useParams()
  const { isPending, isError, data: publications } = useGetModeratorPublications(siteId, page, pageSize, search)

  const { page: queryPage, pageSize: querySize, search: querySearch } = useQueryParams()
  const [searchParams, setSearchParams] = useSearchParams()

  const pagesCount =
    publications?.totalItems && publications.totalItems > 0 ? Math.ceil(publications.totalItems / pageSize) : 0

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
    <div className="flex flex-col">
      <div className="flex justify-between">
        <span>Publications</span>
        <Input placeholder="Search by title or description" value={search} onChange={setSearch} />
        <Select
          items={[
            { label: "10", value: "10" },
            { label: "20", value: "20" },
            { label: "50", value: "50" },
          ]}
          value={pageSize}
          onChange={handlePageSizeChange}
        />
        <Pagination pagesCount={pagesCount} onClick={setPage} page={page} />
      </div>
      <div>
        <table style={{ width: "100%", borderCollapse: "collapse" }}>
          <thead>
            <tr>
              <th>Id</th>
              <th>Category</th>
              <th>Author</th>
              <th>Product</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {isError ? (
              <tr>
                <td>Unable to load</td>
              </tr>
            ) : isPending || !publications ? (
              <tr>
                <td>Loading...</td>
              </tr>
            ) : publications.items.length === 0 ? (
              <tr>
                <td>No publications</td>
              </tr>
            ) : (
              publications.items.map(p => (
                <tr key={p.id}>
                  <td>
                    <Link to={`/${siteId}/m-p/${p.id}`}> {p.id}</Link>
                  </td>
                  <td>{p.categoryId}</td>
                  <td>{p.authorId}</td>
                  <td>{p.productId}</td>
                  <td></td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
