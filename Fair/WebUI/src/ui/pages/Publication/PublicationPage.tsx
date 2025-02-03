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

  if (isPending || !publication) {
    return <div>Loading</div>
  }

  return (
    <div className="flex flex-col gap-3">
      <div className="flex gap-4 text-black">
        <div className="w-1/2 border border-black px-4 py-3 text-center text-purple-500">Logo/Screenshot</div>
        <div className="flex w-1/2 flex-col border border-black px-4 py-3">
          <span>{JSON.stringify(publication)}</span>
          <span>ID: {publication.id}</span>
          <span>CATEGORY ID: {publication.categoryId}</span>
          <span>CREATOR ID: {publication.creatorId}</span>
          <span>PRODUCT ID: {publication.productId}</span>
          <span>PRODUCT NAME: {publication.productName}</span>
          <span>PRODUCT FIELDS: {JSON.stringify(publication.productFields)}</span>
          <span>PRODUCT UPDATED: {publication.productUpdated}</span>
          <span>
            PRODUCT AUTHOR ID: <Link to={`/a/${publication.productAuthorId}`}>{publication.productAuthorId}</Link>
          </span>
          <span>PRODUCT AUTHOR TITLE: {publication.productAuthorTitle}</span>
          <span>SECTIONS: {JSON.stringify(publication.sections)}</span>
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
      <div className="border border-black px-4 py-3 text-black">Description</div>
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
        <Review text="asdfasdfasdf asdfasdfasdf asdfasdfasdf 1" rating={3} userId="123" userName="Lex" />
        <Review text="asdfasdfasdf asdfasdfasdf asdfasdfasdf 2" rating={2} userId="124" userName="Luger" />
        <Review text="asdfasdfasdf asdfasdfasdf asdfasdfasdf 3" rating={4} userId="125" userName="Doinc" />
      </div>
    </div>
  )
}
