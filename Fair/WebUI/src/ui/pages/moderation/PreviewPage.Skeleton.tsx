import { memo } from "react"
import { BreadcrumbsSkeleton } from "ui/components"

import { PublicationPageSkeleton } from "../Publication/PublicationPage.Skeleton"

type PreviewPageSkeletonProps = {
  showBreadcrumbs?: boolean
}

export const PreviewPageSkeleton = memo(({ showBreadcrumbs = false }: PreviewPageSkeletonProps) => (
  <div className="flex flex-col gap-6">
    {showBreadcrumbs && <BreadcrumbsSkeleton items={3} />}
    <PublicationPageSkeleton />
  </div>
))
