import { memo, ReactNode } from "react"
import { useTranslation } from "react-i18next"

import { useStoreContext } from "app"
import { useResolveStoreId } from "hooks"
import { Breadcrumbs, BreadcrumbsItemProps } from "ui/components"
import { routes } from "utils"

export type ModerationHeaderProps = {
  title: string
  breadcrumbTitle?: string
  parentBreadcrumbs?: BreadcrumbsItemProps | BreadcrumbsItemProps[]
  components?: ReactNode
}

export const ModerationHeader = memo(
  ({ title, breadcrumbTitle, parentBreadcrumbs, components }: ModerationHeaderProps) => {
    const storeId = useResolveStoreId()
    const { store: site } = useStoreContext()
    const { t } = useTranslation()

    return (
      <div className="flex min-w-0 flex-col gap-2">
        <div className="flex items-center justify-between">
          <Breadcrumbs
            fullPath={true}
            items={[
              { path: routes.store(storeId!), title: t("common:home") },
              ...(parentBreadcrumbs
                ? Array.isArray(parentBreadcrumbs)
                  ? parentBreadcrumbs
                  : [parentBreadcrumbs]
                : []),
              { title: breadcrumbTitle ?? title },
            ]}
          />
          {site && (
            <span className="text-2xs font-medium leading-5">
              {site.moderatorsIds.length} {t("common:moderators", { count: site.moderatorsIds.length })}
            </span>
          )}
        </div>
        <div className="my-5 flex h-11 items-center justify-between gap-4">
          <div className="flex min-w-0 gap-2 text-3.5xl font-semibold leading-11">
            <span className="truncate">{title}</span>
          </div>
          {components}
        </div>
      </div>
    )
  },
)
