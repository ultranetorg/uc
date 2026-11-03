import { memo } from "react"
import { useTranslation } from "react-i18next"

import { useStoreRolesContext } from "app"
import { Category } from "types"
import { CategoryThreeDotsMenu, ToggleViewButton, ViewType } from "ui/components/specific"
import { formatTitle } from "utils"

export type CategoryHeaderProps = {
  category: Category
  storeId: string
  view: ViewType
  onViewChange(view: ViewType): void
}

export const CategoryHeader = memo(({ category, view, onViewChange }: CategoryHeaderProps) => {
  const { t } = useTranslation("category")
  const { isModerator, isPublisher } = useStoreRolesContext()

  return (
    <div className="flex flex-col gap-2">
      <div className="flex items-center justify-between">
        <div className="flex flex-col">
          <div className="flex gap-2 text-3.5xl font-semibold leading-10">
            <span className="text-gray-800" title={category.title}>
              {formatTitle(category.title)}
            </span>
            <span className="text-gray-400">{category.publicationsCount}</span>
          </div>
          {(isModerator || isPublisher) && (
            <span className="text-2sm leading-5 text-gray-500">{t(`categoryTypes:${category.type}`)}</span>
          )}
        </div>
        <div className="flex items-center gap-4">
          {/* <Pagination onPageChange={page => console.log(page)} page={1} pagesCount={3} /> */}
          {/* <FiltersDropdownButton label={t("filters")} /> */}
          <ToggleViewButton onChange={onViewChange} view={view} gridTitle={t("grid")} listTitle={t("list")} />
          <CategoryThreeDotsMenu categoryId={category.id} categoryTitle={category.title} />
        </div>
      </div>
    </div>
  )
})
