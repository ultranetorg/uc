import { SvgStarXs } from "assets/star-xs"
import { times } from "lodash"

export type RatingBarProps = {
  value: number
  fractionDigits?: number
}

export const RatingBar = ({ value }: RatingBarProps) => (
  <div className="flex gap-2">
    <div className="flex select-none items-center gap-1">
      {times(5).map(i =>
        i < value ? (
          <SvgStarXs className="fill-favorite stroke-favorite" key={i} />
        ) : (
          <SvgStarXs className="stroke-gray-400" key={i} />
        ),
      )}
    </div>
    <span className="text-2sm font-medium leading-4.5 text-gray-800">{value.toFixed(0)}</span>
  </div>
)
