import { SvgXSm } from "assets"

export type ResetAllButtonProps = {
  label: string
  onClick(): void
}

export const ResetAllButton = ({ label, onClick }: ResetAllButtonProps) => (
  <div className="flex cursor-pointer items-center gap-2" onClick={onClick}>
    <SvgXSm className="fill-gray-800" />
    <span className="text-xs leading-3.75 text-gray-750 hover:font-medium">{label}</span>
  </div>
)
