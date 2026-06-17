import { Link } from "react-router-dom"

import { Publication, PublicationExtended } from "types"
import { routes } from "utils"

import { PublicationRow } from "./PublicationRow"

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
      {publications.map(x => (
        <Link className="block" to={routes.publication(siteId!, x.id)} key={x.id}>
          <PublicationRow {...x} />
        </Link>
      ))}
    </div>
  )
}
