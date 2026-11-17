import { memo } from "react"
import { Link } from "react-router-dom"

import { ProductType, Publication, PublicationExtended } from "types"
import { routes } from "utils"

import { PublicationsGridSkeleton } from "./PublicationsGrid.Skeleton"
import { PUBLICATIONS_GRID_CLASSNAME } from "./styles"
import { getCardComponentForCategory } from "./utils"

export type PublicationsGridProps = {
  storeId: string
  isLoading: boolean
  publications?: (Publication | PublicationExtended)[]
  productType: ProductType
}

export const PublicationsGrid = memo(({ storeId, isLoading, publications, productType }: PublicationsGridProps) => {
  if (isLoading) {
    return <PublicationsGridSkeleton />
  }

  const CardComponent = getCardComponentForCategory(productType)

  return (
    <div className={PUBLICATIONS_GRID_CLASSNAME}>
      {publications!.map(x => (
        <Link key={x.id} to={routes.publication(x.id)} className="block min-w-0">
          <CardComponent storeId={storeId} {...x} />
        </Link>
      ))}
    </div>
  )
})
