import { Link, useParams } from "react-router-dom"
import { useDocumentTitle } from "usehooks-ts"

import { useGetAuthor, useGetAuthorPublications } from "entities"

export const AuthorPage = () => {
  const { siteId, authorId } = useParams()

  const { isPending, data: author } = useGetAuthor(authorId)
  const { isPending: isPublicationsPending, data: publications } = useGetAuthorPublications(siteId, author?.id)

  useDocumentTitle(author?.title ? `Author - ${author?.title} | Ultranet Explorer` : "Author | Ultranet Explorer")

  if (isPending || !author) {
    return <div>Loading</div>
  }

  return (
    <div className="flex flex-col text-black">
      <span>ID: {author.id}</span>
      <span>NICKNAME: {author.nickname}</span>
      <span>TITLE: {author.title}</span>
      <span>EXPIRATION: {author.expiration}</span>
      <span>SPACE RESERVED: {author.spaceReserved}</span>
      <span>OWNER ID: {JSON.stringify(author.ownersIds)}</span>
      <h1>PUBLICATIONS:</h1>
      {isPublicationsPending || !publications ? (
        <div>⌛ LOADING PUBLICATIONS</div>
      ) : publications.items.length === 0 ? (
        <div>🚫 NO PUBLICATIONS</div>
      ) : (
        <div className="flex flex-col flex-wrap">
          {publications.items.map(p => (
            <Link to={`/${siteId}/p/${p.id}`}>
              <div className="flex flex-col border border-red-300">
                <span>PRODUCT ID: {p.id}</span>
                <span>TITLE: {p.title}</span>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  )
}
