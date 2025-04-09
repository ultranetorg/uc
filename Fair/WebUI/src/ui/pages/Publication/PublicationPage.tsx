import { useCallback, useState } from "react"
import { Link, useParams } from "react-router-dom"
import { useDocumentTitle } from "usehooks-ts"

import { PAGE_SIZES } from "constants"
import { useGetPublication, useGetReviews } from "entities"
import { Pagination, Select, SelectItem } from "ui/components"

import { Review } from "./Review"

const pageSizes: SelectItem[] = PAGE_SIZES.map(x => ({ label: x.toString(), value: x.toString() }))

export const PublicationPage = () => {
  const { siteId, publicationId } = useParams()
  useDocumentTitle(
    publicationId ? `Publication - ${publicationId} | Ultranet Explorer` : "Publication | Ultranet Explorer",
  )

  const [reviewsPage, setReviewsPage] = useState(0)
  const [reviewPageSize, setReviewsPageSize] = useState(20)

  const { isPending, data: publication } = useGetPublication(publicationId)
  const { isPending: isPendingReviews, data: reviews } = useGetReviews(publicationId, reviewsPage, reviewPageSize)

  const pagesCount = reviews?.totalItems && reviews.totalItems > 0 ? Math.ceil(reviews.totalItems / reviewPageSize) : 0

  const handlePageSizeChange = useCallback((value: string) => {
    setReviewsPage(0)
    setReviewsPageSize(parseInt(value))
  }, [])

  if (isPending || !publication) {
    return <div>Loading</div>
  }

  return (
    <div className="flex flex-col gap-3">
      <div className="flex gap-4 text-black">
        <div className="w-1/2 border border-black px-4 py-3 text-center text-purple-500">Logo/Screenshot</div>
        <div className="flex w-1/2 flex-col border border-black px-4 py-3">
          <span>ID: {publication.id}</span>
          <span>PRODUCT ID: {publication.productId}</span>
          <span>PRODUCT TITLE: {publication.productTitle}</span>

          <span>CATEGORY ID: {publication.categoryId}</span>
          <span>CREATOR ID: {publication.creatorId}</span>
          <span>
            PRODUCT FIELDS: {JSON.stringify(publication.productFields.map(f => ({ ...f, value: atob(f.value) })))}
          </span>
          <span>PRODUCT UPDATED: {publication.productUpdated}</span>
          <span>
            AUTHOR ID: <Link to={`/${siteId}/a/${publication.authorId}`}>{publication.authorId}</Link>
          </span>
          <span>AUTHOR TITLE: {publication.authorTitle}</span>

          <div className="text-purple-500">
            <span>Publisher</span>
            <span>Publisher website link</span>
            <span>Category</span>
            <ul>
              <li>RDN Download link 1</li>
              <li>HTTP Download link 2</li>
              <li>Torrent Download link 3</li>
            </ul>
            <span>RDN Active users</span>
          </div>
        </div>
      </div>
      <div className="border border-black px-4 py-3 text-black">
        <span>DESCRIPTION: {publication.productDescription}</span>
      </div>
      <div className="flex flex-col border border-black px-4 py-3 text-black">
        <span>Platform 1</span>
        <ul className="ml-4">
          <li>Requirements</li>
        </ul>
        <span>Platform 2</span>
        <ul className="ml-4">
          <li>Requirements</li>
        </ul>
        <span>Platform 3</span>
        <ul className="ml-4">
          <li>Requirements</li>
        </ul>
      </div>
      <div className="flex flex-col gap-3">
        <div className="flex items-center justify-between">
          <Select items={pageSizes} value={reviewPageSize} onChange={handlePageSizeChange} />
          <Pagination pagesCount={pagesCount} onClick={setReviewsPage} page={reviewsPage} />
        </div>
        <div>REVIEWS:</div>
        <div>
          {isPendingReviews || !reviews || !reviews.items ? (
            <div>⌛ LOADING REVIEWS</div>
          ) : reviews.items.length === 0 ? (
            <div>🚫 NO REVIEWS</div>
          ) : (
            <div className="flex flex-col gap-3">
              {reviews.items.map(r => (
                <Review
                  key={r.id}
                  text={r.text}
                  rating={r.rating}
                  userId={r.accountId}
                  userName={r.accountNickname || r.accountAddress}
                  created={r.created}
                />
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
