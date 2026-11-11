import { BreadcrumbsSkeleton, SkeletonBlock, SkeletonText } from "ui/components"

const DESCRIPTION_LINES_COUNT = 6

export const AboutPageSkeleton = () => (
  <div className="flex flex-col gap-6">
    <BreadcrumbsSkeleton />
    <div className="flex max-w-160 flex-col gap-6 rounded-lg bg-gray-100 p-6">
      <div className="flex h-10 items-center">
        <SkeletonBlock className="h-8 w-64" rounded="sm" />
      </div>
      <SkeletonText lines={DESCRIPTION_LINES_COUNT} className="py-1" />
    </div>
  </div>
)
