import { memo, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"

import { GridSmSvg, ViewStackedSvg } from "assets"
import { Category } from "types"
import { Breadcrumbs, BreadcrumbsItemProps } from "ui/components"
import { ToggleButton, ToggleButtonItem } from "ui/components/ToggleButton"

import { createBreadcrumbs } from "./utils"

type ViewType = "grid" | "list"

//      fill="#737582" view-stacked
//       fill="#F3F4F9" grid-sm

export type CategoryHeaderProps = {
  siteId: string
  category: Category
}

export const CategoryHeader = memo(({ siteId, category }: CategoryHeaderProps) => {
  const { t } = useTranslation("category")

  const [name, setName] = useState<ViewType>("grid")

  const breadcrumbsItems = useMemo<BreadcrumbsItemProps[]>(
    () => createBreadcrumbs(siteId, category.parentId, category.parentTitle, category.title, t),
    [category.parentId, category.parentTitle, category.title, siteId, t],
  )

  const toggleButtonItems = useMemo<ToggleButtonItem[]>(
    () => [
      { icon: <GridSmSvg />, name: "grid" as ViewType, title: t("grid") },
      { icon: <ViewStackedSvg />, name: "list" as ViewType, title: t("list") },
    ],
    [t],
  )

  const handleChange = useCallback((name: string) => setName(name as ViewType), [])

  return (
    <div className="flex flex-col gap-2">
      <Breadcrumbs items={breadcrumbsItems} />
      <div className="flex items-center justify-between">
        <h2>
          {category.title} <span className="text-stone-800/50">{category.publicationsCount}</span>
        </h2>
        <ToggleButton items={toggleButtonItems} onChange={handleChange} name={name} />
      </div>
    </div>
  )
})
