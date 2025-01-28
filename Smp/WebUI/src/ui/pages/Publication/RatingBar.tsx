import { times } from "lodash"

export type RatingBarProps = {
  rating: number
}

export const RatingBar = ({ rating }: RatingBarProps) => (
  <div className="flex select-none gap-1">
    {times(5).map(i => (i < rating ? <span key={i}>★</span> : <span key={i}>☆</span>))}
  </div>
)
