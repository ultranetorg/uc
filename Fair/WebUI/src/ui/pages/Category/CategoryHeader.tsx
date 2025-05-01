import { memo, useMemo } from "react"
import { useTranslation } from "react-i18next"

import { Category } from "types"
import { Breadcrumbs, BreadcrumbsItemProps } from "ui/components"

export type CategoryHeaderProps = {
  siteId: string
  category: Category
}

export const CategoryHeader = memo(({ siteId, category }: CategoryHeaderProps) => {
  const { t } = useTranslation()

  const breadcrumbsItems = useMemo<BreadcrumbsItemProps[]>(
    () =>
      category.parentId
        ? [
            { path: `/${siteId}`, title: t("home") },
            { path: `/${siteId}/c/${category.parentId}`, title: category.parentTitle },
            { title: category.title },
          ]
        : [{ path: `/${siteId}`, title: t("home") }, { title: category.title }],
    [category.parentId, category.parentTitle, category.title, siteId, t],
  )

  return (
    <div className="flex flex-col gap-2">
      <Breadcrumbs items={breadcrumbsItems} />
      <h2>
        {category.title} <span className="text-stone-800/50">{category.publicationsCount}</span>
      </h2>
    </div>
  )
})
