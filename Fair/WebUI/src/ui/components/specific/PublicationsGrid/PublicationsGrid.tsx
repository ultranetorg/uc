import { ComponentType } from "react"
import { Link } from "react-router-dom"

import { Publication, PublicationExtended } from "types"

import { SoftwarePublicationCard } from "./SoftwarePublicationCard"

export type PublicationsGridProps = {
  siteId: string
  isPending: boolean
  publications?: (Publication | PublicationExtended)[]
  CardComponent?: ComponentType<{ siteId: string } & (Publication | PublicationExtended)>
}

export const PublicationsGrid = ({
  siteId,
  isPending,
  publications,
  CardComponent = SoftwarePublicationCard,
}: PublicationsGridProps) => (
  <div className="flex flex-wrap gap-4">
    {isPending ? (
      <div>⌛ LOADING</div>
    ) : (
      <>
        {publications!.map(x => (
          <Link key={x.id} to={`/${siteId}/p/${x.id}`}>
            <CardComponent siteId={siteId} {...x} />
          </Link>
        ))}
      </>
    )}
  </div>
)
