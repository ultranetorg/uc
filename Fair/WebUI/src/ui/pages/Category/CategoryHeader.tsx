import { memo, useMemo } from "react"
import { useTranslation } from "react-i18next"

import { Category } from "types"
import { Breadcrumbs, BreadcrumbsItemProps, Pagination } from "ui/components"
import { FiltersDropdownButton, ToggleViewButton, ViewType } from "ui/components/specific"
import { createBreadcrumbs } from "utils"

export type CategoryHeaderProps = {
  category: Category
  siteId: string
  view: ViewType
  onViewChange(view: ViewType): void
}

export const CategoryHeader = memo(({ category, siteId, view, onViewChange }: CategoryHeaderProps) => {
  const { t } = useTranslation("category")

  const breadcrumbsItems = useMemo<BreadcrumbsItemProps[]>(
    () => createBreadcrumbs(siteId, category.parentId, category.parentTitle, category.title, t),
    [category.parentId, category.parentTitle, category.title, siteId, t],
  )

  return (
    <div className="flex flex-col gap-2">
      <Breadcrumbs items={breadcrumbsItems} />
      <div className="flex items-center justify-between">
        <div className="flex gap-2 text-3.5xl font-semibold leading-10">
          <span className="text-gray-800">{category.title}</span>
          <span className="text-gray-400">{category.publicationsCount}</span>
        </div>
        <div className="flex items-center gap-4">
          <FiltersDropdownButton label={t("filters")} resetAllLabel={t("resetAll")} />
          <ToggleViewButton onChange={onViewChange} view={view} gridTitle={t("grid")} listTitle={t("list")} />
          <Pagination onPageChange={page => console.log(page)} page={1} pagesCount={3} />
        </div>
      </div>
    </div>
  )
})
