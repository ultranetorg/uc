import { TEST_REVIEW_SRC } from "testConfig"
import { RatingBar } from "ui/components"

export type ReviewProps = {
  text: string
  userId: string
  userName: string
  rating: number
  created: number
}

export const Review = ({ text, userName, rating, created }: ReviewProps) => {
  return (
    <div className="flex flex-col gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6">
      <div className="flex flex-col gap-4">
        <div className="flex gap-4.5">
          <div className="h-13 w-13 overflow-hidden rounded-full">
            <img src={TEST_REVIEW_SRC} className="h-full w-full object-cover" />
          </div>
          <div className="flex flex-col justify-center gap-2">
            <span className="text-2sm font-semibold leading-4.5 text-gray-800">{userName}</span>
            <span className="text-2xs font-medium leading-4 text-gray-500">{created}</span>
          </div>
        </div>
        <RatingBar value={rating} />
      </div>
      <div className="text-2sm leading-5 text-gray-800">{text}</div>
    </div>
  )
}
