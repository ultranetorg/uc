import { Link } from "react-router-dom"

import { ProductType, Publication, PublicationExtended } from "types"
import { routes } from "utils"

import { getCardComponentForCategory } from "./utils"

export type PublicationsGridProps = {
  siteId: string
  isPending: boolean
  publications?: (Publication | PublicationExtended)[]
  productType: ProductType
}

export const PublicationsGrid = ({ siteId, isPending, publications, productType }: PublicationsGridProps) => {
  const CardComponent = getCardComponentForCategory(productType)
  return (
    <div className="flex flex-wrap gap-4">
      {isPending ? (
        <div>Loading</div>
      ) : (
        <>
          {publications!.map(x => (
            <Link key={x.id} to={routes.publication(siteId, x.id)}>
              <CardComponent siteId={siteId} {...x} />
            </Link>
          ))}
        </>
      )}
    </div>
  )
}
