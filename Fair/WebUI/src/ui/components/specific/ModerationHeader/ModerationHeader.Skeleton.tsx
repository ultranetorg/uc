import { memo } from "react"

import { BreadcrumbsSkeleton, ButtonSkeleton, SkeletonBlock } from "ui/components"

type ModerationHeaderSkeletonBaseProps = {
  breadcrumbsCount?: number
  showSubtitle?: boolean
  showActions?: boolean
}

export type ModerationHeaderSkeletonProps = ModerationHeaderSkeletonBaseProps

export const ModerationHeaderSkeleton = memo(
  ({ breadcrumbsCount = 3, showSubtitle = false, showActions = false }: ModerationHeaderSkeletonProps) => (
    <div className="flex min-w-0 flex-col gap-2">
      <BreadcrumbsSkeleton items={breadcrumbsCount} />
      <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between md:gap-4">
        <div className="flex min-w-0 flex-col gap-2">
          <SkeletonBlock className="h-8 w-80 max-w-full sm:h-11" rounded="sm" />
          {showSubtitle && <SkeletonBlock className="my-0.5 h-5 w-120 max-w-full" rounded="xs" />}
        </div>
        {showActions && (
          <div className="flex flex-wrap items-center gap-4">
            <ButtonSkeleton className="w-43.75" />
            <ButtonSkeleton />
          </div>
        )}
      </div>
    </div>
  ),
)
