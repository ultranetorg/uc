import { Link } from "react-router-dom"
import { RatingBar } from "./RatingBar"

export type ReviewProps = {
  text: string
  userId: string
  userName: string
  rating: number
  created: number
}

export const Review = ({ text, userId, userName, rating, created }: ReviewProps) => {
  return (
    <div className="flex flex-col gap-8 border border-black px-4 py-3 text-black">
      <div className="flex justify-between">
        <div>{text}</div>
        <div className="flex flex-col gap-8">
          <RatingBar rating={rating} />
          <Link to={`/users/${userId}`}>{userName}</Link>
        </div>
      </div>
      <span>Created at: {created}</span>
    </div>
  )
}
