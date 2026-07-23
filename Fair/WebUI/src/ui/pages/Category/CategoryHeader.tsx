import { memo, useMemo } from "react"
import { useTranslation } from "react-i18next"

import { Category } from "types"
import { Breadcrumbs, BreadcrumbsItemProps } from "ui/components"
import { ModeratorCategoryMenu, ToggleViewButton, ViewType } from "ui/components/specific"
import { createBreadcrumbs, formatTitle } from "utils"

export type CategoryHeaderProps = {
  category: Category
  storeId: string
  view: ViewType
  onViewChange(view: ViewType): void
}

export const CategoryHeader = memo(({ category, storeId, view, onViewChange }: CategoryHeaderProps) => {
  const { t } = useTranslation("category")

  const breadcrumbsItems = useMemo<BreadcrumbsItemProps[]>(
    () => createBreadcrumbs(storeId, category.path, category.title, t),
    [category.path, category.title, storeId, t],
  )

  return (
    <div className="flex flex-col gap-2">
      <Breadcrumbs items={breadcrumbsItems} />
      <div className="flex items-center justify-between">
        <div className="flex gap-2 text-3.5xl font-semibold leading-10">
          <span className="text-gray-800" title={category.title}>
            {formatTitle(category.title)}
          </span>
          <span className="text-gray-400">{category.publicationsCount}</span>
        </div>
        <div className="flex items-center gap-4">
          {/* <Pagination onPageChange={page => console.log(page)} page={1} pagesCount={3} /> */}
          {/* <FiltersDropdownButton label={t("filters")} resetAllLabel={t("resetAll")} /> */}
          <ToggleViewButton onChange={onViewChange} view={view} gridTitle={t("grid")} listTitle={t("list")} />
          <ModeratorCategoryMenu categoryId={category.id} categoryTitle={category.title} />
        </div>
      </div>
    </div>
  )
})
