import { memo, PropsWithChildren } from "react"

type CommentsSectionHeaderBaseProps = {
  label: string
  totalItems?: number
}

export type CommentsSectionHeaderProps = PropsWithChildren & CommentsSectionHeaderBaseProps

export const CommentsSectionHeader = memo(({ children, label, totalItems }: CommentsSectionHeaderProps) => (
  <div className="flex items-center justify-between">
    <div className="flex gap-2 text-xl leading-6 font-semibold">
      <span className="capitalize">{label}</span>
      {totalItems && <span className="text-gray-500">{totalItems}</span>}
    </div>
    {children}
  </div>
))
