import { Link } from "react-router-dom"

import { CategoryType, Publication, PublicationExtended } from "types"

import { getCardComponentForCategory } from "./utils"

export type PublicationsGridProps = {
  siteId: string
  isPending: boolean
  publications?: (Publication | PublicationExtended)[]
  categoryType: CategoryType
}

export const PublicationsGrid = ({ siteId, isPending, publications, categoryType }: PublicationsGridProps) => {
  const CardComponent = getCardComponentForCategory(categoryType)
  return (
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
}
