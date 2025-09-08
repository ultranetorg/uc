export type CommentsListEmptyStateProps = {
  label: string
}

export const CommentsListEmptyState = ({ label }: CommentsListEmptyStateProps) => (
  <div className="rounded-lg border border-gray-300 bg-gray-100 p-6 text-2sm leading-5 text-gray-800">{label}</div>
)
