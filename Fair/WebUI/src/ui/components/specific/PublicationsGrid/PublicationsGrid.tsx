import { Link } from "react-router-dom"

import { ProductType, Publication, PublicationExtended } from "types"
import { routes } from "utils"

import { getCardComponentForCategory } from "./utils"

export type PublicationsGridProps = {
  storeId: string
  isLoading: boolean
  publications?: (Publication | PublicationExtended)[]
  productType: ProductType
}

export const PublicationsGrid = ({ storeId, isLoading, publications, productType }: PublicationsGridProps) => {
  const CardComponent = getCardComponentForCategory(productType)
  return (
    <div className="flex flex-wrap gap-4">
      {isLoading ? (
        <div>Loading{import.meta.env.DEV ? " (PublicationsGrid)" : ""}</div>
      ) : (
        <>
          {publications!.map(x => (
            <Link key={x.id} to={routes.publication(x.id)}>
              <CardComponent storeId={storeId} {...x} />
            </Link>
          ))}
        </>
      )}
    </div>
  )
}
