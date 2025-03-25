import { useCallback, useEffect, useState } from "react"
import { Link, useParams, useSearchParams } from "react-router-dom"

import { DEFAULT_PAGE_SIZE } from "constants"
import { useGetModeratorReviews } from "entities"
import { useQueryParams } from "hooks"
import { Input, Pagination, Select } from "ui/components"

export const ReviewsTab = () => {
  const [page, setPage] = useState(0)
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE)
  const [search, setSearch] = useState("")

  const { siteId } = useParams()
  const { isPending, isError, data: reviews } = useGetModeratorReviews(siteId, page, pageSize, search)

  const { page: queryPage, pageSize: querySize, search: querySearch } = useQueryParams()
  const [searchParams, setSearchParams] = useSearchParams()

  const pagesCount = reviews?.totalItems && reviews.totalItems > 0 ? Math.ceil(reviews.totalItems / pageSize) : 0

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
        <span>Reviews</span>
        <Input placeholder="Search review" value={search} onChange={setSearch} />
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
