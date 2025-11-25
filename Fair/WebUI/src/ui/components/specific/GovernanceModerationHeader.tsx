import { Breadcrumbs, BreadcrumbsItemProps, ButtonBar, ButtonOutline, ButtonPrimary } from "ui/components"

export type GovernanceModerationHeaderProps = {
  siteId: string
  title: string
  totalItems?: number
  parentBreadcrumb?: BreadcrumbsItemProps
  onCreateButtonClick?: () => void
  onSearchProduct?: () => void
  createButtonLabel?: string
  homeLabel: string
  searchProductLabel?: string
}

export const GovernanceModerationHeader = ({
  siteId,
  title,
  totalItems,
  parentBreadcrumb,
  onCreateButtonClick,
  onSearchProduct,
  createButtonLabel,
  homeLabel,
  searchProductLabel,
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
      <ButtonBar>
        {onSearchProduct && searchProductLabel && (
          <ButtonOutline className="w-48" label={searchProductLabel} onClick={onSearchProduct} />
        )}
        {onCreateButtonClick && createButtonLabel && (
          <ButtonPrimary label={createButtonLabel} onClick={onCreateButtonClick} />
        )}
      </ButtonBar>
    </div>
  </div>
)
