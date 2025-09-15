import { memo, PropsWithChildren } from "react"

import { Breadcrumbs, BreadcrumbsItemProps } from "ui/components"

type PageHeaderBaseProps = {
  siteId: string
  title: string
  totalItems?: number
  parentBreadcrumb?: BreadcrumbsItemProps
  homeLabel: string
}

export type PageHeaderProps = PropsWithChildren & PageHeaderBaseProps

export const PageHeader = memo(
  ({ children, siteId, title, totalItems, parentBreadcrumb, homeLabel }: PageHeaderProps) => (
    <div className="flex flex-col gap-2">
      <Breadcrumbs
        fullPath={true}
        items={[
          { path: `/${siteId}`, title: homeLabel },
          ...(parentBreadcrumb ? [parentBreadcrumb] : []),
          { title: title },
        ]}
      />
      <div className="flex justify-between">
        <div className="flex gap-2 text-3.5xl font-semibold leading-10">
          <span>{title}</span>
          {totalItems && <span className="text-gray-400">({totalItems})</span>}
        </div>
        {children && <div>{children}</div>}
      </div>
    </div>
  ),
)
