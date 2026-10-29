import { memo, ReactNode } from "react"
import { useTranslation } from "react-i18next"

import { Link } from "react-router-dom"
import { useStoreContext, useStoreRolesContext } from "app"
import { useResolveStoreId } from "hooks"
import { Breadcrumbs, BreadcrumbsItemProps } from "ui/components"
import { routes } from "utils"

export type ModerationHeaderProps = {
  title: string
  subtitle?: string
  breadcrumbTitle?: string
  parentBreadcrumbs?: BreadcrumbsItemProps | BreadcrumbsItemProps[]
  components?: ReactNode
  showModeratorsCount?: boolean
}

export const ModerationHeader = memo(
  ({
    title,
    subtitle,
    breadcrumbTitle,
    parentBreadcrumbs,
    components,
    showModeratorsCount = false,
  }: ModerationHeaderProps) => {
    const storeId = useResolveStoreId()
    const { store } = useStoreContext()
    const { isModerator } = useStoreRolesContext()
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
          {store &&
            showModeratorsCount === true &&
            (!isModerator ? (
              <span className="text-2xs font-medium leading-5">
                {store.moderatorsIds.length} {t("common:moderators", { count: store.moderatorsIds.length })}
              </span>
            ) : (
              <Link to={routes.moderation.moderators(storeId!)}>
                <span className="text-2xs font-medium leading-5">
                  {store.moderatorsIds.length} {t("common:moderators", { count: store.moderatorsIds.length })}
                </span>
              </Link>
            ))}
        </div>
        <div className="flex items-center justify-between gap-4">
          <div className="flex min-w-0 flex-col gap-2">
            <span className="truncate text-3.5xl font-semibold leading-11">{title}</span>
            {subtitle && <span className="truncate text-2sm text-gray-500">{subtitle}</span>}
          </div>
          {components}
        </div>
      </div>
    )
  },
)
