import { Link } from "react-router-dom"

import { Publication } from "types"

import { PublicationCard } from "./PublicationCard"

export type PublicationsListProps = {
  siteId: string
  isPending: boolean
  publications: Publication[]
}

export const PublicationsList = ({ siteId, isPending, publications }: PublicationsListProps) => (
  <div className="flex gap-8">
    {isPending ? (
      <div>⌛ LOADING</div>
    ) : publications.length === 0 ? (
      <div>🚫 NO CATEGORIES</div>
    ) : (
      <>
        {publications.map(x => (
          <Link key={x.id} to={`/${siteId}/p/${x.id}`}>
            <PublicationCard key={x.id} publicationName={x.productTitle} authorTitle={x.productDescription} />
          </Link>
        ))}
      </>
    )}
  </div>
)
