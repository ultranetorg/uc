import { memo } from "react"
import { times } from "lodash"

import { SvgStarXs } from "assets/star-xs"
import { formatRating } from "utils"

export type RatingBarProps = {
  value: number
}

export const RatingBar = memo(({ value }: RatingBarProps) => {
  const starsCount = Math.floor(value / 10)
  const formattedRating = formatRating(value)

  return (
    <div className="flex items-center gap-2" title={formattedRating}>
      <div className="flex select-none items-center gap-1">
        {times(5).map(i =>
          i < starsCount ? (
            <SvgStarXs className="fill-favorite stroke-favorite" key={i} />
          ) : (
            <SvgStarXs className="stroke-gray-400" key={i} />
          ),
        )}
      </div>
      <span className="text-2sm font-medium leading-4.5 text-gray-800">{formattedRating}</span>
    </div>
  )
})
