import { useCallback, useEffect } from "react"
import { Link, useParams } from "react-router-dom"

import { PAGE_SIZES } from "constants"
import { useGetModeratorPublications } from "entities"
import { Input, Pagination, Select, SelectItem } from "ui/components"
import { usePagePagination } from "ui/pages/hooks"

const pageSizes: SelectItem[] = PAGE_SIZES.map(x => ({ label: x.toString(), value: x.toString() }))

export const PublicationsTab = () => {
  const { page, setPage, pageSize, setPageSize, search, setSearch, resetPagination } = usePagePagination()

  const { siteId } = useParams()
  const { isPending, isError, data: publications } = useGetModeratorPublications(siteId, page, pageSize, search)

  const pagesCount =
    publications?.totalItems && publications.totalItems > 0 ? Math.ceil(publications.totalItems / pageSize) : 0

  useEffect(() => {
    return () => {
      resetPagination()
    }
  }, [resetPagination])

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
      <div className="flex justify-between">
        <span>Publications</span>
        <Input placeholder="Search by title or description" value={search} onChange={setSearch} />
        <Select items={pageSizes} value={pageSize} onChange={handlePageSizeChange} />
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
