import { twMerge } from "tailwind-merge"
import { SvgEyeSm } from "assets"
import { Breadcrumbs, BreadcrumbsItemProps, ButtonBar, ButtonOutline, ButtonPrimary, Separator } from "ui/components"

import { PropsWithClassName } from "types"
import { SoftwarePublicationLogo } from "./SoftwarePublicationLogo"

export type ModeratorPublicationHeaderBaseProps = {
  siteId: string
  showLogo?: boolean
  logoFileId?: string
  title: string
  parentBreadcrumb?: BreadcrumbsItemProps
  onApprove?: () => void
  onReject?: () => void
  onPreview?: () => void
  homeLabel: string
}

export type ModeratorPublicationHeaderProps = PropsWithClassName & ModeratorPublicationHeaderBaseProps

export const ModeratorPublicationHeader = ({
  className,
  siteId,
  showLogo = true,
  logoFileId,
  title,
  parentBreadcrumb,
  onApprove,
  onReject,
  onPreview,
  homeLabel,
}: ModeratorPublicationHeaderProps) => (
  <div className={twMerge("flex flex-col gap-6", className)}>
    <Breadcrumbs
      fullPath={true}
      items={[
        { path: `/${siteId}`, title: homeLabel },
        ...(parentBreadcrumb ? [parentBreadcrumb] : []),
        { title: title },
      ]}
    />
    <div className="flex h-11 items-center justify-between">
      <SoftwarePublicationLogo showLogo={showLogo} logoFileId={logoFileId} title={title} />
      <ButtonBar className="items-center">
        {onApprove && <ButtonPrimary className="h-11" label="Suggest to approve" onClick={onApprove} />}
        {onReject && <ButtonOutline className="h-11" label="Suggest to reject" onClick={onReject} />}
        {(onApprove || onReject) && onPreview && <Separator className="h-8" />}
        {onReject && (
          <ButtonOutline
            className="h-11 w-52"
            label="Preview publication"
            iconBefore={<SvgEyeSm className="fill-gray-800" />}
            onClick={onPreview}
          />
        )}
      </ButtonBar>
    </div>
  </div>
)
