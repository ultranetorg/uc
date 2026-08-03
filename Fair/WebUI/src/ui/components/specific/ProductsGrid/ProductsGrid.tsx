import { memo, useEffect, useState } from "react"
import { Link } from "react-router-dom"
import { sampleSize } from "lodash"

import { routes } from "utils"

import { GridCard, GridCardProps } from "./GridCard"

export type ProductsGridItem = {
  publicationId: string
  hasMorePublications: boolean
} & GridCardProps

export type ProductsGridProps = {
  items: ProductsGridItem[]
  randomRefreshCount?: number
  onShowAllClick?: (productId: string) => void
}

const RANDOM_REFRESH_INTERVAL_MS = 3000
const DEFAULT_RANDOM_REFRESH_COUNT = 5

export const ProductsGrid = memo(
  ({ items, randomRefreshCount = DEFAULT_RANDOM_REFRESH_COUNT, onShowAllClick }: ProductsGridProps) => {
    const [refreshTriggers, setRefreshTriggers] = useState<Record<string, number>>({})

    useEffect(() => {
      if (items.length === 0) return

      const intervalId = setInterval(() => {
        const picked = sampleSize(items, randomRefreshCount)
        setRefreshTriggers(prev => {
          const next = { ...prev }
          picked.forEach(item => {
            next[item.publicationId] = (next[item.publicationId] ?? 0) + 1
          })
          return next
        })
      }, RANDOM_REFRESH_INTERVAL_MS)

      return () => clearInterval(intervalId)
    }, [items, randomRefreshCount])

    return (
      <div className="flex flex-col gap-3">
        <div className="flex justify-center">
          <div className="flex size-full max-w-[1248px] flex-wrap items-center justify-center gap-6">
            {items.map(x => (
              <Link to={routes.publication("", x.publicationId)} key={x.publicationId}>
                <GridCard
                  productId={x.productId}
                  productTitle={x.productTitle}
                  authorTitle={x.authorTitle}
                  avatarId={x.avatarId}
                  storesRatings={x.storesRatings}
                  refreshTrigger={refreshTriggers[x.publicationId]}
                  onShowAllClick={x.hasMorePublications ? onShowAllClick : undefined}
                />
              </Link>
            ))}
          </div>
        </div>
      </div>
    )
  },
)
