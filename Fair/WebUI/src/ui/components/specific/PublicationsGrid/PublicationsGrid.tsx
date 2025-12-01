import { Link } from "react-router-dom"

import { ProductType, Publication, PublicationExtended } from "types"

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
        <div>⌛ LOADING</div>
      ) : (
        <>
          {publications!.map(x => (
            <Link key={x.id} to={`/${siteId}/p/${x.id}/${productType}`}>
              <CardComponent siteId={siteId} {...x} />
            </Link>
          ))}
        </>
      )}
    </div>
  )
}
