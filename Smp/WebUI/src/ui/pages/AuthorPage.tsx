import { useParams } from "react-router-dom"
import { useDocumentTitle } from "usehooks-ts"

import { useGetAuthor } from "entities"

export const AuthorPage = () => {
  const { authorId } = useParams()

  const { isPending, data: author } = useGetAuthor(authorId)

  useDocumentTitle(author?.title ? `Author - ${author?.title} | Ultranet Explorer` : "Author | Ultranet Explorer")

  if (isPending || !author) {
    return <div>Loading</div>
  }

  return (
    <div className="flex flex-col text-black">
      <span>ID: {author.id}</span>
      <span>TITLE: {author.title}</span>
      <span>OWNER ID: {author.ownerId}</span>
      <span>EXPIRATION: {author.expiration}</span>
      <span>SPACE RESERVED: {author.spaceReserved}</span>
      <span>SPACE USED: {author.spaceUsed}</span>
    </div>
  )
}
