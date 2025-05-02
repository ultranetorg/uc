import { Link } from "react-router-dom"

import { Publication, PublicationExtended } from "types"

import { PublicationCard } from "./PublicationCard"

export type PublicationsListProps = {
  siteId: string
  isPending: boolean
  publications: (Publication | PublicationExtended)[]
}

export const PublicationsList = ({ siteId, isPending, publications }: PublicationsListProps) => (
  <div className="flex flex-wrap gap-8">
    {isPending ? (
      <div>⌛ LOADING</div>
    ) : (
      <>
        {publications.map(x => (
          <Link key={x.id} to={`/${siteId}/p/${x.id}`}>
            <PublicationCard siteId={siteId} {...x} />
          </Link>
        ))}
      </>
    )}
  </div>
)
