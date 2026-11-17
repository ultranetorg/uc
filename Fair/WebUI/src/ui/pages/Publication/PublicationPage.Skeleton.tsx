import { CommentsListSkeleton, SkeletonBlock, SkeletonText } from "ui/components"

const INFO_ROWS_COUNT = 5
const ABOUT_LINES_COUNT = 3

export const PublicationPageSkeleton = () => (
  // Publication page
  <div className="flex flex-col gap-6">
    {/* Software publication logo */}
    <div className="flex items-center gap-3 sm:gap-4">
      {/* Publication logo */}
      <SkeletonBlock className="size-14 shrink-0 sm:size-17" rounded="2xl" />
      {/* Title */}
      <SkeletonBlock className="h-8 w-3/5 max-w-72" rounded="sm" />
    </div>

    <div className="flex flex-col gap-6 lg:max-w-187.5 lg:gap-8">
      <div className="flex flex-col gap-4 rounded-lg border border-gray-300 bg-gray-100 p-4 sm:p-6">
        {Array.from({ length: INFO_ROWS_COUNT }, (_, i) => (
          <div key={i} className="flex flex-col gap-2 sm:flex-row sm:gap-6">
            <div className="sm:w-40 sm:shrink-0">
              <SkeletonBlock className="my-0.5 h-3 w-24" rounded="xs" />
            </div>
            <SkeletonBlock className="my-0.5 h-4 w-36 max-w-full" rounded="xs" />
          </div>
        ))}
        <div className="flex flex-col gap-2">
          <SkeletonBlock className="my-0.5 h-3 w-24" rounded="xs" />
          <SkeletonText lines={ABOUT_LINES_COUNT} />
        </div>
      </div>
      <div className="flex flex-col gap-4">
        <div className="flex items-center justify-between gap-3">
          <SkeletonBlock className="h-6 w-20" rounded="xs" />
          <SkeletonBlock className="h-9.5 w-35 shrink-0" rounded="sm" />
        </div>
        <CommentsListSkeleton count={2} />
      </div>
    </div>
  </div>
)
