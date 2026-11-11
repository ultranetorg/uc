import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"
import { ButtonSkeleton, SkeletonBlock } from "ui/components"

const DEFAULT_COUNT = 2
const DESCRIPTION_LINES_COUNT = 2

type OptionsCollapsesListSkeletonBaseProps = {
  count?: number
  expanded?: boolean
}

export type OptionsCollapsesListSkeletonProps = OptionsCollapsesListSkeletonBaseProps & PropsWithClassName

export const OptionsCollapsesListSkeleton = memo(
  ({ className, count = DEFAULT_COUNT, expanded = false }: OptionsCollapsesListSkeletonProps) => (
    <div className={twMerge("flex flex-col gap-3", className)}>
      {Array.from({ length: count }, (_, i) => (
        <div key={i} className="flex flex-col overflow-hidden rounded-lg border border-gray-300 bg-gray-100">
          <div
            className={twMerge(
              "box-border flex h-14 items-center justify-between gap-4 p-4",
              expanded && "border-b border-b-gray-300",
            )}
          >
            <SkeletonBlock className="h-4 w-40 max-w-full" rounded="xs" />
            <div className="flex shrink-0 items-center gap-2">
              <SkeletonBlock className="h-4 w-24" rounded="xs" />
              <SkeletonBlock className="size-5 shrink-0" rounded="sm" />
            </div>
          </div>

          {expanded && (
            <div className="flex flex-col gap-4 p-4">
              <div className="flex flex-col">
                {Array.from({ length: DESCRIPTION_LINES_COUNT }, (_, line) => (
                  <SkeletonBlock
                    key={line}
                    className={twMerge("my-0.5 h-4 w-full", line === DESCRIPTION_LINES_COUNT - 1 && "w-3/5")}
                    rounded="xs"
                  />
                ))}
              </div>
              <hr className="h-px border-0 bg-gray-300" />
              <ButtonSkeleton className="w-37.5 self-end" />
            </div>
          )}
        </div>
      ))}
    </div>
  ),
)
