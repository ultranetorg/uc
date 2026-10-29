import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type VotesProgressBaseProps = {
  votes: number
  votesRequired: number
  title?: string
}

export type VotesProgressProps = PropsWithClassName & VotesProgressBaseProps

export const VotesProgress = memo(({ className, votes, votesRequired, title }: VotesProgressProps) => {
  const percents = votesRequired > 0 ? Math.min(100, (votes * 100) / votesRequired) : 0

  return (
    <div className={twMerge("flex items-center gap-2", className)} title={title}>
      <div className="h-1.5 w-full min-w-10 overflow-hidden rounded-full bg-gray-200">
        <div
          className={twMerge("h-full rounded-full bg-gray-500", votes >= votesRequired && "bg-light-green")}
          style={{ width: `${percents}%` }}
        />
      </div>
      <span className="whitespace-nowrap">
        {votes} / {votesRequired}
      </span>
    </div>
  )
})
