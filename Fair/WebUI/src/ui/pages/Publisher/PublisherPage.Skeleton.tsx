import { BreadcrumbsSkeleton, TableSectionSkeleton } from "ui/components"
import { AuthorProfileSkeleton } from "ui/components/author"

type PublisherPageSkeletonProps = {
  showBreadcrumbs?: boolean
}

export const PublisherPageSkeleton = ({ showBreadcrumbs = false }: PublisherPageSkeletonProps) => (
  <>
    {showBreadcrumbs && <BreadcrumbsSkeleton items={3} />}
    <AuthorProfileSkeleton />
    <TableSectionSkeleton withCategory />
  </>
)
