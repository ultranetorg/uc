import { times } from "lodash"

export type RatingBarProps = {
  value: number
}

export const RatingBar = ({ value }: RatingBarProps) => (
  <div className="flex gap-2">
    <div className="flex select-none gap-1">
      {times(5).map(i => (i < value ? <span key={i}>★</span> : <span key={i}>☆</span>))}
    </div>
    <span>{value.toFixed(1)}</span>
  </div>
)
