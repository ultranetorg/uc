import { forwardRef, memo } from "react"
import { useTranslation } from "react-i18next"

import { SvgX } from "assets"
import { PropsWithStyle, StoreBase } from "types"

import { FavoriteStoresMenuItem } from "./FavoriteStoresMenuItem"

type FavoriteStoresMenuBaseProps = {
  items: StoreBase[]
  onClose: () => void
}

export type FavoriteStoresMenuProps = PropsWithStyle & FavoriteStoresMenuBaseProps

export const FavoriteStoresMenu = memo(
  forwardRef<HTMLDivElement, FavoriteStoresMenuProps>(({ style, items, onClose }, ref) => {
    const { t } = useTranslation("favoriteStores")

    return (
      <div
        className="z-10 flex max-h-205 w-120 flex-col divide-y divide-gray-700 overflow-hidden rounded-lg border bg-gray-800 shadow-md"
        ref={ref}
        style={style}
      >
        <div className="flex shrink-0 items-center justify-between gap-4 p-4">
          <span className="select-none text-2base leading-5 text-white">{t("title")}</span>
          <SvgX className="cursor-pointer stroke-gray-300 hover:stroke-white" onClick={onClose} />
        </div>
        <div className="flex min-h-0 flex-col gap-1 overflow-y-auto p-2">
          {items.map(x => (
            <FavoriteStoresMenuItem
              key={x.id}
              storeId={x.id}
              title={x.title}
              fileId={x.imageFileId}
              onClick={onClose}
            />
          ))}
        </div>
      </div>
    )
  }),
)
