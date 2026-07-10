import { useTranslation } from "react-i18next"
import { memo } from "react"

import { SvgSoftwareLogo } from "assets/fallback"
import { ButtonOutline, ImageFallback } from "ui/components"
import { buildFileUrl } from "utils"

export type ProductTableRowProps = {
  id: string
  title?: string
  logoId?: string
  publicationsCount: number
  onProductStoresClick: (id: string) => void
}

export const ProductTableRow = memo(
  ({ id, title, logoId, publicationsCount, onProductStoresClick }: ProductTableRowProps) => {
    const { t } = useTranslation("profile")

    return (
      <div className="flex items-center justify-between p-4 text-2sm leading-5">
        <div className="flex w-[43%] items-center gap-2">
          <div className="size-8 shrink-0 overflow-hidden rounded-lg">
            <ImageFallback
              src={buildFileUrl(logoId)}
              fallback={<SvgSoftwareLogo className="size-8" />}
              className="size-full object-cover"
            />
          </div>
          <span className="font-medium">{title ?? "Product has no title yet"}</span>
        </div>
        <div className="flex w-[27%] justify-center">
          <ButtonOutline
            className="h-9"
            label={`${publicationsCount} ${t("publication", { count: publicationsCount })}`}
            disabled={publicationsCount === 0}
            onClick={() => onProductStoresClick(id)}
          />
        </div>
      </div>
    )
  },
)
