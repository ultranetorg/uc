import { SvgStarXs } from "assets/star-xs"
import { times } from "lodash"

export type YourRatingBarProps = {
  value: number
}

export const YourRatingBar = ({ value }: YourRatingBarProps) => (
  <div className="flex gap-2">
    <div className="flex select-none items-center gap-1">
      {times(5).map(i =>
        i < value ? (
          <SvgStarXs className="h-9 w-9 fill-favorite stroke-favorite" key={i} />
        ) : (
          <SvgStarXs className="h-9 w-9 stroke-gray-400" key={i} />
        ),
      )}
    </div>
  </div>
)
