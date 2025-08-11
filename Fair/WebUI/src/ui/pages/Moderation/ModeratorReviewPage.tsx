import { useParams } from "react-router-dom"

import { useGetReviewProposal } from "entities"

export const ModeratorReviewPage = () => {
  const { siteId, reviewId } = useParams()

  const { isPending, data: review } = useGetReviewProposal(siteId, reviewId)

  if (isPending || !review) {
    return <div>Loading...</div>
  }

  return (
    <div className="flex flex-col">
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
            <tr key={review.id}>
              <td>{review.id}</td>
              {/* <td>{review.publicationId}</td> */}
              <td></td>
              <td></td>
              {/* <td>{review.text}</td>
              <td>{review.rating}</td>
              <td>{review.created}</td> */}
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  )
}
