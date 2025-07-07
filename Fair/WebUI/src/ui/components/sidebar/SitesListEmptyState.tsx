export type SitesListEmptyStateProps = {
  message: string
}

export const SitesListEmptyState = ({ message }: SitesListEmptyStateProps) => (
  <span className="bg-warning rounded p-2 text-2xs leading-4">{message}</span>
)
