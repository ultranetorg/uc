import { useParams } from "react-router-dom"

import { useGetPublicationProposal } from "entities"

export const ModeratorPublicationPage = () => {
  const { publicationId } = useParams()

  const { isPending, data: publication } = useGetPublicationProposal(publicationId)

  if (isPending || !publication) {
    return <div>Loading...</div>
  }

  return (
    <div className="flex flex-col">
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
          <tr key={publication.id}>
            <td>{publication.id}</td>
            <td>{publication.categoryId}</td>
            <td>{publication.authorId}</td>
            <td>{publication.productId}</td>
            <td></td>
          </tr>
        </tbody>
      </table>
    </div>
  )
}
