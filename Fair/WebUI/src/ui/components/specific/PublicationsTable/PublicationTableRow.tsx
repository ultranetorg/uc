import { useTranslation } from "react-i18next"
import { Link } from "react-router-dom"
import { memo } from "react"

import { SvgSoftwareLogo } from "assets/fallback"
import { useResolveStoreId } from "hooks"
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
    const storeId = useResolveStoreId()
    const { t } = useTranslation("profile")

    return (
      <div className="flex items-center justify-between p-4 text-2sm leading-5">
        <div className="w-[43%]">
          <Link to={routes.publication(storeId!, id)} className="flex w-fit items-center gap-2">
            <div className="size-8 shrink-0 overflow-hidden rounded-lg">
              <ImageFallback
                src={buildFileUrl(logoId)}
                fallback={<SvgSoftwareLogo className="size-8" />}
                className="size-full object-cover"
              />
            </div>
            <span className="font-medium">{title}</span>
          </Link>
        </div>
        <span className="w-[30%]">
          <Link to={routes.category(storeId!, categoryId)}>{categoryTitle}</Link>
        </span>
        <div className="flex w-[27%] justify-center">
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
