import { forwardRef, memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type ProfileButtonBaseProps = {
  label: string
  iconBefore?: JSX.Element
  iconAfter?: JSX.Element
  onClick?: () => void
}

export type ProfileButtonProps = PropsWithClassName & ProfileButtonBaseProps

export const ProfileButton = memo(
  forwardRef<HTMLDivElement, ProfileButtonProps>(
    ({ className, label, iconBefore, iconAfter, onClick, ...rest }, ref) => (
      <div
        className={twMerge(
          "box-border flex h-12 w-full cursor-pointer items-center gap-2 rounded border border-gray-300 bg-gray-100 py-3 pl-4 pr-3 text-2sm leading-5 text-gray-800 hover:bg-gray-200",
          className,
        )}
        onClick={onClick}
        ref={ref}
        {...rest}
      >
        {iconBefore}
        <span className="grow">{label}</span>
        {iconAfter}
      </div>
    ),
  ),
)
