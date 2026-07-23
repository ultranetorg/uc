export type StoresListEmptyStateProps = {
  message: string
}

export const StoresListEmptyState = ({ message }: StoresListEmptyStateProps) => (
  <span className="rounded bg-warning p-2 text-2xs leading-4">{message}</span>
)
