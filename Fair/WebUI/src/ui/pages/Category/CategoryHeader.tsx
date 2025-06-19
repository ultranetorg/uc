import { memo, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"

import { Category } from "types"
import { Breadcrumbs, BreadcrumbsItemProps } from "ui/components"
import { ToggleViewButton, ViewType } from "ui/components/specific"

import { createBreadcrumbs } from "./utils"

export type CategoryHeaderProps = {
  siteId: string
  category: Category
}

export const CategoryHeader = memo(({ siteId, category }: CategoryHeaderProps) => {
  const { t } = useTranslation("category")

  const [view, setView] = useState<ViewType>("grid")

  const breadcrumbsItems = useMemo<BreadcrumbsItemProps[]>(
    () => createBreadcrumbs(siteId, category.parentId, category.parentTitle, category.title, t),
    [category.parentId, category.parentTitle, category.title, siteId, t],
  )

  const handleChange = useCallback((name: string) => setView(name as ViewType), [])

  return (
    <div className="flex flex-col gap-2">
      <Breadcrumbs items={breadcrumbsItems} />
      <div className="flex items-center justify-between">
        <h2>
          {category.title} <span className="text-stone-800/50">{category.publicationsCount}</span>
        </h2>
        <ToggleViewButton onChange={handleChange} view={view} gridTitle={t("grid")} listTitle={t("list")} />
      </div>
    </div>
  )
})
