import { memo, useCallback, useEffect, useMemo, useRef, useState } from "react"
import { useNavigate } from "react-router-dom"

import { SvgSoftwareLogo } from "assets/fallback"
import { DropdownItem, ImageFallback } from "ui/components"
import { buildFileUrl, routes } from "utils"

import { StoreRatingDropdown, StoreRatingDropdownItem } from "./StoreRatingDropdown"

export interface CardStoreStoreRating {
  storeId: string
  storeTitle: string
  publicationId: string
  rating: number
}

export type GridCardProps = {
  productId: string
  productTitle: string
  authorTitle: string
  avatarId?: string
  storesRatings: CardStoreStoreRating[]
  refreshTrigger?: number
  onShowAllClick?: (productId: string) => void
}

const CHANGE_FLASH_DURATION_MS = 700

const pickRandomStoreId = (storesRatings: CardStoreStoreRating[]): string | undefined =>
  storesRatings.length ? storesRatings[Math.floor(Math.random() * storesRatings.length)].storeId : undefined

export const GridCard = memo(
  ({
    productId,
    productTitle,
    authorTitle,
    avatarId,
    storesRatings,
    refreshTrigger,
    onShowAllClick,
  }: GridCardProps) => {
    const navigate = useNavigate()

    const [selectedStoreId, setSelectedStoreId] = useState(() => pickRandomStoreId(storesRatings))
    const [isFlashing, setIsFlashing] = useState(false)

    const flashTimeoutRef = useRef<ReturnType<typeof setTimeout>>()
    const isDropdownOpenRef = useRef(false)

    const items = useMemo(
      () =>
        storesRatings.map(x => ({
          value: x.storeId,
          label: x.storeTitle,
          rating: x.rating,
          publicationId: x.publicationId,
        })),
      [storesRatings],
    )

    const handleChange = useCallback(
      (item: DropdownItem) => {
        const { publicationId } = item as StoreRatingDropdownItem
        navigate(routes.publication("", publicationId))
      },
      [navigate],
    )

    const handleShowAllClick = useCallback(() => onShowAllClick!(productId), [onShowAllClick, productId])

    useEffect(() => {
      if (storesRatings.length === 0 || isDropdownOpenRef.current) return

      const storeId = pickRandomStoreId(storesRatings)
      setSelectedStoreId(storeId)

      setIsFlashing(true)
      clearTimeout(flashTimeoutRef.current)
      flashTimeoutRef.current = setTimeout(() => setIsFlashing(false), CHANGE_FLASH_DURATION_MS)

      return () => clearTimeout(flashTimeoutRef.current)
    }, [storesRatings, refreshTrigger])

    return (
      <div
        className={`flex h-53.5 w-55 flex-col gap-4 rounded-lg bg-gray-100 px-2 py-6 hover:bg-gray-200`}
        title={productTitle}
      >
        <div className="mx-auto size-18 overflow-hidden rounded-2xl">
          <ImageFallback src={buildFileUrl(avatarId)} fallback={<SvgSoftwareLogo className="size-18" />} />
        </div>
        <div className="flex w-51 flex-col gap-2 text-center">
          <span className="truncate text-2sm font-semibold leading-4.5 text-gray-800">{productTitle}</span>
          <span className="overflow-hidden truncate text-2xs leading-4 text-gray-500">{authorTitle}</span>
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
