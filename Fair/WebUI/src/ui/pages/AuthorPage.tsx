import { Link, useParams } from "react-router-dom"
import { useDocumentTitle } from "usehooks-ts"

import { useGetSiteAuthor } from "entities"

export const AuthorPage = () => {
  const { authorId } = useParams()

  const { isPending, data: author } = useGetSiteAuthor(authorId)

  useDocumentTitle(author?.title ? `Author - ${author?.title} | Ultranet Explorer` : "Author | Ultranet Explorer")

  if (isPending || !author) {
    return <div>Loading</div>
  }

  return (
    <div className="flex flex-col text-black">
      <span>ID: {author.id}</span>
      <span>TITLE: {author.title}</span>
      <span>EXPIRATION: {author.expiration}</span>
      <span>SPACE RESERVED: {author.spaceReserved}</span>
      <span>SPACE USED: {author.spaceUsed}</span>
      <span>OWNER ID: {author.ownerId}</span>
      {author.publications ? (
        <>
          <h2>PUBLICATIONS:</h2>
          <div className="flex flex-wrap">
            {author.publications.map(p => (
              <Link to={`/p/${p.id}`}>
                <div className="flex flex-col border border-red-300">
                  <span>PRODUCT ID: {p.productId}</span>
                  <span>TITLE: {p.productTitle}</span>
                  <span>DESCRIPTION: {p.productDescription}</span>
                </div>
              </Link>
            ))}
          </div>
        </>
      ) : (
        <h2>🚫 NO PUBLICATIONS</h2>
      )}
    </div>
  )
}
