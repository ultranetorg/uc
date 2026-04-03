import { memo, ReactNode } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { Breadcrumbs, BreadcrumbsItemProps } from "ui/components"

export type ModerationHeaderProps = {
  title: string
  breadcrumbTitle?: string
  parentBreadcrumbs?: BreadcrumbsItemProps | BreadcrumbsItemProps[]
  components?: ReactNode
}

export const ModerationHeader = memo(
  ({ title, breadcrumbTitle, parentBreadcrumbs, components }: ModerationHeaderProps) => {
    const { siteId } = useParams()
    const { t } = useTranslation()

    return (
      <div className="flex min-w-0 flex-col gap-2 overflow-hidden">
        <Breadcrumbs
          fullPath={true}
          items={[
            { path: `/${siteId}`, title: t("common:home") },
            ...(parentBreadcrumbs ? (Array.isArray(parentBreadcrumbs) ? parentBreadcrumbs : [parentBreadcrumbs]) : []),
            { title: breadcrumbTitle ?? title },
          ]}
        />
        <div className="flex h-11 justify-between gap-4">
          <div className="flex min-w-0 gap-2 text-3.5xl font-semibold leading-11">
            <span className="truncate">{title}</span>
          </div>
          {components}
        </div>
      </div>
    )
  },
)
