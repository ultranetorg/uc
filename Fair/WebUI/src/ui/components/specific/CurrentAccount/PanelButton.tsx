import { ComponentPropsWithoutRef, forwardRef, memo, ReactNode } from "react"

type WithLabel = { label: string; children?: never }
type WithChildren = { label?: never; children: ReactNode }

export interface ProfileButtonNewBaseProps {
  iconBefore?: ReactNode
  iconAfter?: ReactNode
}

export type ProfileButtonNewProps = (WithLabel | WithChildren) &
  ProfileButtonNewBaseProps &
  Omit<ComponentPropsWithoutRef<"div">, "children">

export const PanelButton = memo(
  forwardRef<HTMLDivElement, ProfileButtonNewProps>(({ children, label, iconBefore, iconAfter, ...rest }, ref) => (
    <div
      className="flex h-full max-w-53.5 min-w-0 cursor-pointer items-center gap-2 rounded-md bg-gray-600 p-1 text-white hover:bg-gray-550 sm:w-53.5"
      ref={ref}
      {...rest}
    >
      {iconBefore}
      {children ? children : <span className="flex-1 truncate text-2sm leading-5 select-none">{label}</span>}
      {iconAfter && <span className="flex shrink-0">{iconAfter}</span>}
    </div>
  )),
)
