import { PropsWithChildren } from "react"

type CommentsSectionHeaderBaseProps = {
  label: string
  totalItems?: number
}

export type CommentsSectionHeaderProps = PropsWithChildren & CommentsSectionHeaderBaseProps

export const CommentsSectionHeader = ({ children, label, totalItems }: CommentsSectionHeaderProps) => (
  <div className="flex items-center justify-between">
    <div className="flex gap-2 text-xl font-semibold leading-6">
      <span className="capitalize text-gray-800">{label}</span>
      {totalItems && <span className="text-gray-500">{totalItems}</span>}
    </div>
    {children}
  </div>
)
