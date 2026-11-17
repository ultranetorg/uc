import { useTranslation } from "react-i18next"
import { Link } from "react-router-dom"
import { memo } from "react"

import { SvgSoftwareLogo3Xl } from "assets/fallback"
import { ButtonOutline, ImageFallback } from "ui/components"
import { buildFileUrl, routes } from "utils"

export type PublicationTableRowProps = {
  id: string
  productId: string
  title: string
  logoId?: string
  publicationsCount: number
  categoryId: string
  categoryTitle: string
  onProductStoresClick: (id: string) => void
}

export const PublicationTableRow = memo(
  ({
    id,
    productId,
    title,
    logoId,
    publicationsCount,
    categoryId,
    categoryTitle,
    onProductStoresClick,
  }: PublicationTableRowProps) => {
    const { t } = useTranslation("profile")

    return (
      <div className="flex flex-col items-start gap-3 p-4 text-2sm leading-5 sm:flex-row sm:items-center sm:justify-between">
        <div className="w-full min-w-0 sm:w-[43%]">
          <Link to={routes.publication(id)} className="flex w-fit max-w-full items-center gap-2">
            <div className="size-8 shrink-0 overflow-hidden rounded-lg">
              <ImageFallback
                src={buildFileUrl(logoId)}
                fallback={<SvgSoftwareLogo3Xl className="size-8" />}
                className="size-full object-cover"
              />
            </div>
            <span className="font-medium">{title}</span>
          </Link>
        </div>
        <span className="w-full min-w-0 sm:w-[30%]">
          <Link to={routes.category(categoryId)}>{categoryTitle}</Link>
        </span>
        <div className="flex w-full sm:w-[27%] sm:justify-center">
          <ButtonOutline
            className="h-9"
            label={`${publicationsCount} ${t("publication", { count: publicationsCount })}`}
            disabled={publicationsCount === 0}
            onClick={() => onProductStoresClick(productId)}
          />
        </div>
      </div>
    )
  },
)
