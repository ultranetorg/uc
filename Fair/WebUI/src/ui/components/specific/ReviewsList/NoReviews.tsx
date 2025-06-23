export type NoReviewsProps = {
  label: string
}

export const NoReviews = ({ label }: NoReviewsProps) => (
  <div className="rounded-lg border border-gray-300 bg-gray-100 p-6 text-2sm leading-5 text-gray-800">{label}</div>
)
