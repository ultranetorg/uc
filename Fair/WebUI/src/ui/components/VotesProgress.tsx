import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type VotesProgressBaseProps = {
  votes: number
  votesRequired: number
  title?: string
}

export type VotesProgressProps = PropsWithClassName & VotesProgressBaseProps

type Rgb = [number, number, number]

const COLOR_GRAY: Rgb = [107, 114, 128] // gray-500
const COLOR_YELLOW: Rgb = [225, 161, 7] // light-yellow
const COLOR_GREEN: Rgb = [68, 184, 72] // light-green

const mix = (from: Rgb, to: Rgb, ratio: number) =>
  from.map((value, i) => Math.round(value + (to[i] - value) * ratio)).join(", ")

// 0% - gray, 50% - yellow, 100% - green.
const getProgressColor = (percents: number) =>
  percents < 50
    ? `rgb(${mix(COLOR_GRAY, COLOR_YELLOW, percents / 50)})`
    : `rgb(${mix(COLOR_YELLOW, COLOR_GREEN, (percents - 50) / 50)})`

export const VotesProgress = memo(({ className, votes, votesRequired, title }: VotesProgressProps) => {
  const percents = votesRequired > 0 ? Math.min(100, (votes * 100) / votesRequired) : 0

  return (
    <div className={twMerge("flex items-center gap-2", className)} title={title}>
      <div className="h-1.5 w-full min-w-10 overflow-hidden rounded-full bg-gray-200">
        <div
          className="h-full rounded-full"
          style={{ width: `${percents}%`, backgroundColor: getProgressColor(percents) }}
        />
      </div>
      <span className="whitespace-nowrap">
        {votes} / {votesRequired}
      </span>
    </div>
  )
})
