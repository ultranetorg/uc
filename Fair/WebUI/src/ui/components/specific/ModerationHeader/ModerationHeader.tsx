import { memo, ReactNode } from "react"
import { useTranslation } from "react-i18next"

import { useResolveStoreId } from "hooks"
import { Breadcrumbs, BreadcrumbsItemProps } from "ui/components"
import { routes } from "utils"

export type ModerationHeaderProps = {
  title: string
  subtitle?: string
  breadcrumbTitle?: string
  parentBreadcrumbs?: BreadcrumbsItemProps | BreadcrumbsItemProps[]
  components?: ReactNode
}

export const ModerationHeader = memo(
  ({ title, subtitle, breadcrumbTitle, parentBreadcrumbs, components }: ModerationHeaderProps) => {
    const storeId = useResolveStoreId()
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
        </div>
        <div className="flex items-center justify-between gap-4">
          <div className="flex min-w-0 flex-col gap-2">
            <span className="truncate text-3.5xl leading-11 font-semibold">{title}</span>
            {subtitle && <span className="truncate text-2sm leading-6 text-gray-500">{subtitle}</span>}
          </div>
          {components}
        </div>
      </div>
    )
  },
)
