import { Link } from "react-router-dom"

import { ProductType, Publication, PublicationExtended } from "types"
import { routes } from "utils"

import { getCardComponentForCategory } from "./utils"

export type PublicationsGridProps = {
  storeId: string
  isPending: boolean
  publications?: (Publication | PublicationExtended)[]
  productType: ProductType
}

export const PublicationsGrid = ({ storeId, isPending, publications, productType }: PublicationsGridProps) => {
  const CardComponent = getCardComponentForCategory(productType)
  return (
    <div className="flex flex-wrap gap-4">
      {isPending ? (
        <div>Loading</div>
      ) : (
        <>
          {publications!.map(x => (
            <Link key={x.id} to={routes.publication(storeId, x.id)}>
              <CardComponent siteId={storeId} {...x} />
            </Link>
          ))}
        </>
      )}
    </div>
  )
}
