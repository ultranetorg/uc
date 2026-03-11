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
      <div className="flex flex-col gap-2">
        <Breadcrumbs
          fullPath={true}
          items={[
            { path: `/${siteId}`, title: t("common:home") },
            ...(parentBreadcrumbs ? (Array.isArray(parentBreadcrumbs) ? parentBreadcrumbs : [parentBreadcrumbs]) : []),
            { title: breadcrumbTitle ?? title },
          ]}
        />
        <div className="flex justify-between">
          <div className="flex gap-2 text-3.5xl font-semibold leading-10">{title}</div>
          {components}
        </div>
      </div>
    )
  },
)
