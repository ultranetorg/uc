import { Breadcrumbs, BreadcrumbsItemProps, ButtonPrimary } from "ui/components"

export type GovernanceModerationHeaderProps = {
  siteId: string
  title: string
  totalItems?: number
  parentBreadcrumb?: BreadcrumbsItemProps
  onCreateButtonClick?: () => void
  createButtonLabel?: string
  homeLabel: string
}

export const GovernanceModerationHeader = ({
  siteId,
  title,
  totalItems,
  parentBreadcrumb,
  onCreateButtonClick,
  createButtonLabel,
  homeLabel,
}: GovernanceModerationHeaderProps) => (
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
      {onCreateButtonClick && createButtonLabel && (
        <ButtonPrimary label={createButtonLabel} onClick={onCreateButtonClick} />
      )}
    </div>
  </div>
)
