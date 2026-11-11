import { memo } from "react"

import { SkeletonBlock } from "ui/components"

const LINKS_COUNT = 3

export type AuthorProfileSkeletonProps = {
  showRoles?: boolean
}

const SectionLabel = () => <SkeletonBlock className="my-0.5 h-3 w-12" rounded="xs" />

export const AuthorProfileSkeleton = memo(({ showRoles = true }: AuthorProfileSkeletonProps) => (
  <div className="flex items-start gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6">
    <SkeletonBlock className="size-30 shrink-0" rounded="full" />
    <div className="flex min-w-0 flex-1 flex-col gap-6">
      <div className="flex w-full items-start gap-3">
        <div className="flex min-w-0 flex-col gap-1">
          <SkeletonBlock className="h-6 w-45" rounded="xs" />
          <SkeletonBlock className="h-4 w-16" rounded="xs" />
        </div>
      </div>
      <div className="flex flex-col gap-2">
        <SectionLabel />
        <div className="flex flex-wrap gap-2">
          {Array.from({ length: LINKS_COUNT }, (_, i) => (
            <SkeletonBlock key={i} className="h-10 w-55 shrink-0" rounded="sm" />
          ))}
        </div>
      </div>
      {showRoles && (
        <div className="flex flex-col gap-2">
          <SectionLabel />
          <SkeletonBlock className="my-1 h-4 w-20" rounded="xs" />
        </div>
      )}
    </div>
  </div>
))
