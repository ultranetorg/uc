import { TableSectionSkeleton } from "ui/components"
import { AuthorProfileSkeleton } from "ui/components/author"

export const AuthorPageSkeleton = () => (
  <>
    <AuthorProfileSkeleton showRoles={false} />
    <TableSectionSkeleton />
  </>
)
