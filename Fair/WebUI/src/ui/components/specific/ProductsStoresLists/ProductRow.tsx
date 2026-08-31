import { memo, useCallback, useEffect, useMemo, useRef, useState } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { SvgSoftwareLogo } from "assets/fallback"
import { ProductPublication, ProductSearchResult } from "types"
import { DropdownItem, ImageFallback } from "ui/components"
import { buildFileUrl, routes } from "utils"

import { StoreRatingDropdown, StoreRatingDropdownItem } from "./StoreRatingDropdown"
import {
  COLUMN_CELL_CLASSNAME,
  COLUMN_NAME_CLASSNAME,
  COLUMN_STORE_CLASSNAME,
  COLUMN_TYPE_CLASSNAME,
  ROW_CLASSNAME,
} from "./styles"

const CHANGE_FLASH_DURATION_MS = 700

const pickRandomStoreId = (publications: ProductPublication[]): string | undefined =>
  publications.length ? publications[Math.floor(Math.random() * publications.length)].storeId : undefined

type ProductRowBaseProps = {
  refreshTrigger?: number
  onShowAllClick?: (productId: string) => void
}

export type ProductRowProps = Omit<ProductSearchResult, "hasMorePublications"> & ProductRowBaseProps

export const ProductRow = memo(
  ({
    productId,
    productLogoId,
    productTitle,
    productType,
    authorTitle,
    publications,
    refreshTrigger,
    onShowAllClick,
  }: ProductRowProps) => {
    const navigate = useNavigate()
    const { t } = useTranslation()

    const [selectedStoreId, setSelectedStoreId] = useState(() => pickRandomStoreId(publications))
    const [isFlashing, setIsFlashing] = useState(false)

    const flashTimeoutRef = useRef<ReturnType<typeof setTimeout>>()
    const isDropdownOpenRef = useRef(false)
    const isRowHoveredRef = useRef(false)

    const items = useMemo(
      () =>
        publications.map(x => ({
          value: x.storeId,
          label: x.storeTitle,
          rating: x.rating,
          publicationId: x.publicationId,
        })),
      [publications],
    )

    const handleChange = useCallback(
      (item: DropdownItem) => {
        const { publicationId } = item as StoreRatingDropdownItem
        navigate(routes.publication(publicationId))
      },
      [navigate],
    )

    const handleShowAllClick = useCallback(() => onShowAllClick!(productId), [onShowAllClick, productId])

    useEffect(() => {
      if (publications.length === 0 || isDropdownOpenRef.current || isRowHoveredRef.current) return

      const storeId = pickRandomStoreId(publications)
      setSelectedStoreId(storeId)

      if (publications.length < 2) return

      setIsFlashing(true)
      clearTimeout(flashTimeoutRef.current)
      flashTimeoutRef.current = setTimeout(() => setIsFlashing(false), CHANGE_FLASH_DURATION_MS)

      return () => clearTimeout(flashTimeoutRef.current)
    }, [publications, refreshTrigger])

    return (
      <div
        className={ROW_CLASSNAME}
        title={productTitle}
        onMouseEnter={() => {
          isRowHoveredRef.current = true
        }}
        onMouseLeave={() => {
          isRowHoveredRef.current = false
        }}
      >
        <div className={COLUMN_NAME_CLASSNAME}>
          <div className="size-8 flex-none overflow-hidden rounded-lg">
            <ImageFallback
              src={buildFileUrl(productLogoId)}
              fallback={<SvgSoftwareLogo className="size-full object-cover" />}
            />
          </div>
          <span className="truncate text-2xs leading-4 text-gray-900">{productTitle}</span>
        </div>
        <span className={COLUMN_TYPE_CLASSNAME}>{productType !== "none" && t("categoryTypes:" + productType)}</span>
        <span className={COLUMN_CELL_CLASSNAME} title={authorTitle}>
          {authorTitle}
        </span>
        <div className={COLUMN_STORE_CLASSNAME}>
          <StoreRatingDropdown
            items={items}
            value={selectedStoreId}
            isFlashing={isFlashing}
            onChange={handleChange}
            onOpenChange={open => {
              isDropdownOpenRef.current = open
            }}
            onShowAllClick={onShowAllClick ? handleShowAllClick : undefined}
          />
        </div>
      </div>
    )
  },
)
