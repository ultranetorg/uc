import { memo } from "react"
import { Link } from "react-router-dom"

import { routes } from "utils"
import { GridCard, GridCardProps } from "./GridCard"

export type ProductsGridItem = {
  publicationId: string
} & GridCardProps

export type ProductsGridProps = {
  items: ProductsGridItem[]
}

export const ProductsGrid = memo(({ items }: ProductsGridProps) => (
  <div className="flex flex-col gap-3">
    <div className="flex justify-center">
      <div className="flex size-full max-w-[1248px] flex-wrap items-center justify-center gap-6">
        {items.map(x => (
          <Link to={routes.publication("", x.publicationId)} key={x.publicationId}>
            <GridCard productTitle={x.productTitle} authorTitle={x.authorTitle} avatarId={x.avatarId} />
          </Link>
        ))}
      </div>
    </div>
  </div>
))
