import { times } from "lodash"
import { useState } from "react"
import { SvgStarXs } from "assets/star-xs"

export type YourRatingBarProps = {
  value: number
  onRatingChange: (rating: number) => void
}

export const YourRatingBar = ({ value, onRatingChange }: YourRatingBarProps) => {
  const [hovered, setHovered] = useState<number | undefined>()

  const displayed = hovered ?? value

  return (
    <div className="flex gap-2">
      <div className="flex select-none items-center gap-1" onMouseLeave={() => setHovered(undefined)}>
        {times(5).map(i => (
          <SvgStarXs
            key={i}
            className={`size-9 cursor-pointer transition-colors ${
              i < displayed ? "fill-favorite stroke-favorite" : "stroke-gray-400"
            }`}
            onMouseEnter={() => setHovered(i + 1)}
            onClick={() => onRatingChange(i + 1)}
          />
        ))}
      </div>
    </div>
  )
}
