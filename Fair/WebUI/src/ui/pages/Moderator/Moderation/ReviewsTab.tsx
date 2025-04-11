import { useCallback, useEffect } from "react"
import { Link, useParams } from "react-router-dom"

import { PAGE_SIZES } from "constants"
import { useGetModeratorReviews } from "entities"
import { Input, Pagination, Select, SelectItem } from "ui/components"
import { usePagePagination } from "ui/pages/hooks"

const pageSizes: SelectItem[] = PAGE_SIZES.map(x => ({ label: x.toString(), value: x.toString() }))

export const ReviewsTab = () => {
  const { page, setPage, pageSize, setPageSize, search, setSearch, resetPagination } = usePagePagination()

  const { siteId } = useParams()
  const { isPending, isError, data: reviews } = useGetModeratorReviews(siteId, page, pageSize, search)

  const pagesCount = reviews?.totalItems && reviews.totalItems > 0 ? Math.ceil(reviews.totalItems / pageSize) : 0

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
        <span>Reviews</span>
        <Input placeholder="Search review" value={search} onChange={setSearch} />
        <Select items={pageSizes} value={pageSize} onChange={handlePageSizeChange} />
        <Pagination pagesCount={pagesCount} onClick={setPage} page={page} />
      </div>
      <div>
        <table style={{ width: "100%", borderCollapse: "collapse" }}>
          <thead>
            <tr>
              <th>Id</th>
              <th>Publication</th>
              <th>Author</th>
              <th>Commented By</th>
              <th>Text</th>
              <th>Text New</th>
              <th>Rating</th>
              <th>Created At</th>
            </tr>
          </thead>
          <tbody>
            {isError ? (
              <tr>
                <td>Unable to load</td>
              </tr>
            ) : isPending || !reviews ? (
              <tr>
                <td>Loading...</td>
              </tr>
            ) : reviews.items.length === 0 ? (
              <tr>
                <td>No reviews</td>
              </tr>
            ) : (
              reviews.items.map(review => (
                <tr key={review.id}>
                  <td>
                    <Link to={`/${siteId}/m-r/${review.id}`}> {review.id}</Link>
                  </td>
                  <td>{review.publicationId}</td>
                  <td></td>
                  <td></td>
                  <td>{review.text}</td>
                  <td>{review.textNew}</td>
                  <td>{review.rating}</td>
                  <td>{review.created}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
