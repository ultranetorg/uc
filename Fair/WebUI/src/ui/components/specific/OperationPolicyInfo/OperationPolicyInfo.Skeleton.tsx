import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"
import { SkeletonBlock } from "ui/components"

import { INFO_GRID_CLASSNAME } from "./styles"

const ROWS_COUNT = 2

type OperationPolicyInfoSkeletonBaseProps = {
  title?: string
}

export type OperationPolicyInfoSkeletonProps = PropsWithClassName & OperationPolicyInfoSkeletonBaseProps

export const OperationPolicyInfoSkeleton = memo(({ className, title }: OperationPolicyInfoSkeletonProps) => (
  <div
    className={twMerge(
      "flex w-full flex-col items-start gap-3 rounded-lg text-2xs leading-4 md:flex-row md:justify-between md:gap-8",
      className,
    )}
  >
    {title && <span className="text-2sm leading-5 font-semibold">{title}</span>}
    <dl className={INFO_GRID_CLASSNAME}>
      {Array.from({ length: ROWS_COUNT }, (_, i) => (
        <div key={i} className="contents">
          <dt className="py-1">
            <SkeletonBlock className="h-4 w-26" rounded="xs" />
          </dt>
          <dd className="py-1">
            <SkeletonBlock className="h-4 w-35" rounded="xs" />
          </dd>
          <dt className="py-1">
            <SkeletonBlock className="h-4 w-26" rounded="xs" />
          </dt>
          <dd className="py-1">
            <SkeletonBlock className="h-4 w-6" rounded="xs" />
          </dd>
        </div>
      ))}
    </dl>
  </div>
))
