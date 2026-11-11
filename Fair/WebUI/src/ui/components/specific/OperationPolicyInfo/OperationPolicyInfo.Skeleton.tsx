import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"
import { SkeletonBlock } from "ui/components"

const ROWS_COUNT = 2

type OperationPolicyInfoSkeletonBaseProps = {
  title?: string
}

export type OperationPolicyInfoSkeletonProps = PropsWithClassName & OperationPolicyInfoSkeletonBaseProps

export const OperationPolicyInfoSkeleton = memo(({ className, title }: OperationPolicyInfoSkeletonProps) => (
  <div className={twMerge("flex w-full items-start justify-between gap-8 rounded-lg text-2xs leading-4", className)}>
    {title && <span className="text-2sm leading-5 font-semibold text-gray-800">{title}</span>}
    <table className="ml-auto border-collapse">
      <tbody>
        {Array.from({ length: ROWS_COUNT }, (_, i) => (
          <tr key={i}>
            <th className="w-29 py-1">
              <SkeletonBlock className="h-4 w-26" rounded="xs" />
            </th>
            <td className="w-50 py-1">
              <SkeletonBlock className="h-4 w-35" rounded="xs" />
            </td>
            <th className="w-29 py-1">
              <SkeletonBlock className="h-4 w-26" rounded="xs" />
            </th>
            <td className="w-6 py-1">
              <SkeletonBlock className="h-4 w-6" rounded="xs" />
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  </div>
))
