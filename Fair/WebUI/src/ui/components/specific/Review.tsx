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
    <div className="flex flex-col gap-6 rounded-lg border border-zinc-700 p-6">
      <div className="flex flex-col gap-4">
        <div className="flex justify-between py-[6px]">
          <div className="flex gap-4">
            <div className="h-14 w-14 rounded-full bg-neutral-300" />
            <div className="flex flex-col justify-between py-[6px]">
              <div>{userName}</div>
              <div>{created}</div>
            </div>
          </div>
          <div>REPLY</div>
        </div>
        <RatingBar value={rating} />
      </div>
      <div className="text-sm">{text}</div>
      {/* <div className="flex justify-between">
        <div>{text}</div>
        <div className="flex flex-col gap-8">
          <RatingBar rating={rating} />
          <Link to={`/users/${userId}`}>{userName}</Link>
        </div>
      </div>
      <span>Created at: {created}</span> */}
    </div>
  )
}
