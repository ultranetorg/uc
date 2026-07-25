import { Link } from "react-router-dom"

import { Publication, PublicationExtended } from "types"
import { routes } from "utils"

import { PublicationRow } from "./PublicationRow"

export type PublicationsListProps = {
  isLoading?: boolean
  storeId?: string
  publications?: (Publication | PublicationExtended)[]
}

export const PublicationsList = ({ isLoading, storeId, publications }: PublicationsListProps) => {
  if (isLoading || !publications) {
    return <div>Loading</div>
  }

  return (
    <div className="divide-y divide-gray-300 overflow-hidden rounded-lg border border-gray-300">
      {publications.map(x => (
        <Link className="block" to={routes.publication(storeId!, x.id)} key={x.id}>
          <PublicationRow {...x} />
        </Link>
      ))}
    </div>
  )
}
