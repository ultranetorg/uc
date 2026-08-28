import { memo, useEffect, useState } from "react"
import { useTranslation } from "react-i18next"
import { Link } from "react-router-dom"
import { sampleSize } from "lodash"

import { ProductSearchResult } from "types"
import { List, ListHeader } from "ui/components"
import { routes } from "utils"

import { ProductRow } from "./ProductRow"
import { COLUMN_CELL_CLASSNAME, COLUMN_NAME_CLASSNAME, COLUMN_STORE_CLASSNAME, COLUMN_TYPE_CLASSNAME } from "./styles"

const RANDOM_REFRESH_INTERVAL_MS = 3000
const DEFAULT_RANDOM_REFRESH_COUNT = 4

export type ProductsListProps = {
  items: ProductSearchResult[]
  randomRefreshCount?: number
  onShowAllClick?: (productId: string) => void
}

export const ProductsList = memo(({ items, randomRefreshCount = DEFAULT_RANDOM_REFRESH_COUNT }: ProductsListProps) => {
  const { t } = useTranslation("common")

  const [refreshTriggers, setRefreshTriggers] = useState<Record<string, number>>({})

  useEffect(() => {
    if (items.length === 0) return

    const intervalId = setInterval(() => {
      const picked = sampleSize(items, randomRefreshCount)
      setRefreshTriggers(prev => {
        const next = { ...prev }
        picked.forEach(item => {
          next[item.publications[0].publicationId] = (next[item.publications[0].publicationId] ?? 0) + 1
        })
        return next
      })
    }, RANDOM_REFRESH_INTERVAL_MS)

    return () => clearInterval(intervalId)
  }, [items, randomRefreshCount])

  return (
    <List
      header={
        <ListHeader className="capitalize leading-4.25">
          <span className={COLUMN_NAME_CLASSNAME}>{t("product")}</span>
          <span className={COLUMN_TYPE_CLASSNAME}>{t("type")}</span>
          <span className={COLUMN_CELL_CLASSNAME}>{t("author")}</span>
          <span className={COLUMN_STORE_CLASSNAME}>{t("store")}</span>
        </ListHeader>
      }
      className="text-2xs leading-4"
    >
      {items.map(x => (
        <Link
          className="block"
          to={routes.publication(x.publications[0].publicationId)}
          key={x.publications[0].publicationId}
        >
          <ProductRow {...x} refreshTrigger={refreshTriggers[x.publications[0].publicationId]} />
        </Link>
      ))}
    </List>
  )
})
