import { memo, ReactNode } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export type ButtonOutlineBaseProps = {
  disabled?: boolean
  iconAfter?: ReactNode
  iconBefore?: ReactNode
  label: string
  iconPosition?: "before" | "after"
  onClick?: () => void
}

export type ButtonOutlineProps = PropsWithClassName & ButtonOutlineBaseProps

export const ButtonOutline = memo(
  ({ className, disabled, label, iconAfter, iconBefore, onClick }: ButtonOutlineProps) => (
    <span
      className={twMerge(
        "box-border flex w-fit select-none items-center justify-center gap-2 rounded border px-4 py-2 text-2sm leading-5",
        className,
        disabled !== true
          ? "cursor-pointer border-gray-400 text-gray-800 hover:border-gray-950"
          : "cursor-default border-gray-200 text-gray-400",
      )}
      onClick={disabled !== true ? onClick : undefined}
    >
      {iconBefore} {label} {iconAfter}
    </span>
  ),
)
