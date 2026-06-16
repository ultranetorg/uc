import { PropsWithClassName } from "types"

type TableEmptyStateBaseProps = {
  message: string
}

export type TableEmptyStateProps = PropsWithClassName & TableEmptyStateBaseProps

export const TableEmptyState = ({ className, message }: TableEmptyStateProps) => (
  <div className={className}>{message}</div>
)
