import { SvgXSm } from "assets"

export type ResetAllButtonProps = {
  label: string
  onClick(): void
}

export const ResetAllButton = ({ label, onClick }: ResetAllButtonProps) => (
  <div className="flex cursor-pointer items-center gap-2" onClick={onClick}>
    <SvgXSm className="stroke-gray-750" />
    <span className="text-gray-750 text-xs font-medium leading-3.75 hover:font-semibold">{label}</span>
  </div>
)
