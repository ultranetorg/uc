import { Breadcrumbs, BreadcrumbsItemProps } from "ui/components"
import { SoftwarePublicationLogo } from "./SoftwarePublicationLogo"

export type ModeratorPublicationHeaderProps = {
  siteId: string
  logoFileId?: string
  title: string
  parentBreadcrumb?: BreadcrumbsItemProps
  homeLabel: string
}

export const ModeratorPublicationHeader = ({
  siteId,
  logoFileId,
  title,
  parentBreadcrumb,
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
    <SoftwarePublicationLogo logoFileId={logoFileId} title={title} />
  </div>
)
