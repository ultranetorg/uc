import { Publication, PublicationExtended } from "types"

import { PublicationRow } from "./PublicationRow"
import { Link } from "react-router-dom"

export type PublicationsListProps = {
  isLoading?: boolean
  siteId?: string
  publications?: (Publication | PublicationExtended)[]
}

export const PublicationsList = ({ isLoading, siteId, publications }: PublicationsListProps) => {
  if (isLoading || !publications) {
    return <div className="text-gray-500">⌛ LOADING</div>
  }

  return (
    <div className="divide-y divide-gray-300 overflow-hidden rounded-lg border border-gray-300">
      {publications.map(({ id, ...rest }) => (
        <Link to={`/${siteId}/p/${id}`} key={id}>
          <PublicationRow {...rest} />
        </Link>
      ))}
    </div>
  )
}
