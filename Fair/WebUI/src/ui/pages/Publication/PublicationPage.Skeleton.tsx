import { CommentsListSkeleton, SkeletonBlock, SkeletonText } from "ui/components"

const INFO_ROWS_COUNT = 5
const ABOUT_LINES_COUNT = 3

export const PublicationPageSkeleton = () => (
  <div className="flex flex-col gap-6">
    <div className="flex items-center gap-4">
      <SkeletonBlock className="size-17 shrink-0" rounded="2xl" />
      <SkeletonBlock className="h-8 w-72" rounded="sm" />
    </div>
    <div className="flex max-w-187.5 flex-col gap-8">
      <div className="flex flex-col gap-4 rounded-lg border border-gray-300 bg-gray-100 p-6">
        {Array.from({ length: INFO_ROWS_COUNT }, (_, i) => (
          <div key={i} className="flex gap-6">
            <div className="w-40 shrink-0">
              <SkeletonBlock className="my-0.5 h-3 w-24" rounded="xs" />
            </div>
            <SkeletonBlock className="my-0.5 h-4 w-36" rounded="xs" />
          </div>
        ))}
        <div className="flex flex-col gap-2">
          <SkeletonBlock className="my-0.5 h-3 w-24" rounded="xs" />
          <SkeletonText lines={ABOUT_LINES_COUNT} />
        </div>
      </div>
      <div className="flex flex-col gap-4">
        <div className="flex items-center justify-between">
          <SkeletonBlock className="h-6 w-20" rounded="xs" />
          <SkeletonBlock className="h-9.5 w-35" rounded="sm" />
        </div>
        <CommentsListSkeleton count={2} />
      </div>
    </div>
  </div>
)
