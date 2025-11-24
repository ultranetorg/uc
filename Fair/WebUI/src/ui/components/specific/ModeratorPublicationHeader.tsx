import { SvgEyeSm } from "assets"
import { Breadcrumbs, BreadcrumbsItemProps, ButtonBar, ButtonOutline, ButtonPrimary, Separator } from "ui/components"

import { SoftwarePublicationLogo } from "./SoftwarePublicationLogo"

export type ModeratorPublicationHeaderProps = {
  siteId: string
  logoFileId?: string
  title: string
  parentBreadcrumb?: BreadcrumbsItemProps
  onApprove: () => void
  onReject: () => void
  onPreview: () => void
  homeLabel: string
}

export const ModeratorPublicationHeader = ({
  siteId,
  logoFileId,
  title,
  parentBreadcrumb,
  onApprove,
  onReject,
  onPreview,
  homeLabel,
}: ModeratorPublicationHeaderProps) => (
  <div className="flex flex-col gap-6">
    <Breadcrumbs
      fullPath={true}
      items={[
        { path: `/${siteId}`, title: homeLabel },
        ...(parentBreadcrumb ? [parentBreadcrumb] : []),
        { title: title },
      ]}
    />
    <div className="flex items-center justify-between">
      <SoftwarePublicationLogo logoFileId={logoFileId} title={title} />
      <ButtonBar className="items-center">
        <ButtonPrimary className="h-11" label="Suggest to approve" onClick={onApprove} />
        <ButtonOutline className="h-11" label="Suggest to reject" onClick={onReject} />
        <Separator className="h-8" />
        <ButtonOutline
          className="h-11 w-52"
          label="Preview publication"
          iconBefore={<SvgEyeSm className="fill-gray-800" />}
          onClick={onPreview}
        />
      </ButtonBar>
    </div>
  </div>
)
