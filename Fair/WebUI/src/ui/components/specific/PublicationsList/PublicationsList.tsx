import { Link } from "react-router-dom"

import { Publication, PublicationExtended } from "types"
import { routes } from "utils"

import { PublicationRow } from "./PublicationRow"
import { PublicationsListSkeleton } from "./PublicationsList.Skeleton"

export type PublicationsListProps = {
  isLoading?: boolean
  publications?: (Publication | PublicationExtended)[]
  showPublicationType?: boolean
}

export const PublicationsList = ({ isLoading, publications, showPublicationType }: PublicationsListProps) => {
  if (isLoading || !publications) {
    return <PublicationsListSkeleton showPublicationType={showPublicationType} />
  }

  return (
    <div className="divide-y divide-gray-300 overflow-hidden rounded-lg border border-gray-300">
      {publications.map(x => (
        <Link className="block" to={routes.publication(x.id)} key={x.id}>
          <PublicationRow {...x} showPublicationType={showPublicationType} />
        </Link>
      ))}
    </div>
  )
}
