import { forwardRef, memo, ReactNode } from "react"

type WithLabel = { label: string; children?: never }
type WithChildren = { label?: never; children: ReactNode }

export interface ProfileButtonNewBaseProps {
  iconBefore?: ReactNode
  iconAfter?: ReactNode
  onClick?: () => void
}

export type ProfileButtonNewProps = (WithLabel | WithChildren) & ProfileButtonNewBaseProps

export const PanelButton = memo(
  forwardRef<HTMLDivElement, ProfileButtonNewProps>(({ children, label, iconBefore, iconAfter, onClick }, ref) => (
    <div
      className="flex h-full w-53.5 cursor-pointer items-center gap-2 rounded-md bg-gray-600 p-1 text-white hover:bg-gray-550"
      onClick={onClick}
      ref={ref}
    >
      {iconBefore}
      {children ? children : <span className="flex-1 select-none text-2sm leading-5">{label}</span>}
      {iconAfter}
    </div>
  )),
)
