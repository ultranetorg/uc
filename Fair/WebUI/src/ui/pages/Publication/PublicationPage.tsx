import { Link, useParams } from "react-router-dom"
import { useDocumentTitle } from "usehooks-ts"

import { useGetPublication } from "entities"

import { Review } from "./Review"

export const PublicationPage = () => {
  const { publicationId } = useParams()
  useDocumentTitle(
    publicationId ? `Publication - ${publicationId} | Ultranet Explorer` : "Publication | Ultranet Explorer",
  )

  const { isPending, data: publication } = useGetPublication(publicationId)
  console.log(JSON.stringify(publication))

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
            AUTHOR ID: <Link to={`/a/${publication.authorId}`}>{publication.authorId}</Link>
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
      {publication.reviews ? (
        <div className="flex flex-col gap-3">
          {publication.reviews.map(r => (
            <Review key={r.id} text={r.text} rating={r.rating} userId={r.accountId} userName={r.accountAddress} />
          ))}
        </div>
      ) : (
        <div className="flex flex-col gap-3">🚫 NO REVIEWS</div>
      )}
    </div>
  )
}
