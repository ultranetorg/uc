import { BreadcrumbsSkeleton } from "ui/components"
import { UserDetailsViewSkeleton } from "ui/views"

type ReviewerPageSkeletonProps = {
  showBreadcrumbs?: boolean
}

export const ReviewerPageSkeleton = ({ showBreadcrumbs = false }: ReviewerPageSkeletonProps) => (
  <div className="flex flex-col gap-6">
    {showBreadcrumbs && <BreadcrumbsSkeleton items={3} />}
    <UserDetailsViewSkeleton />
  </div>
)
