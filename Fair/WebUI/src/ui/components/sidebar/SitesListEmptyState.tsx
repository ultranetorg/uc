export type SitesListEmptyStateProps = {
  message: string
}

export const SitesListEmptyState = ({ message }: SitesListEmptyStateProps) => (
  <span className="rounded bg-warning p-2 text-2xs leading-4">{message}</span>
)
