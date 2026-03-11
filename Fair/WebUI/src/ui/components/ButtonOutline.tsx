import { memo, MouseEvent, ReactNode } from "react"
import { twMerge } from "tailwind-merge"

import { SvgSpinner } from "assets"
import { PropsWithClassName } from "types"

export type ButtonOutlineBaseProps = {
  disabled?: boolean
  iconAfter?: ReactNode
  iconBefore?: ReactNode
  label: string
  loading?: boolean
  iconPosition?: "before" | "after"
  onClick?: (e: MouseEvent<HTMLSpanElement>) => void
}

export type ButtonOutlineProps = PropsWithClassName & ButtonOutlineBaseProps

export const ButtonOutline = memo(
  ({ className, disabled, label, loading = false, iconAfter, iconBefore, onClick }: ButtonOutlineProps) => {
    const isDisabled = disabled || loading
    return (
      <span
        className={twMerge(
          "box-border flex w-fit select-none items-center justify-center gap-2 rounded border px-4 py-2 text-2sm leading-5",
          className,
          !isDisabled
            ? "cursor-pointer border-gray-400 text-gray-800 hover:border-gray-950"
            : "cursor-default border-gray-200 text-gray-400",
          loading && "py-2",
        )}
        onClick={!isDisabled ? onClick : undefined}
      >
        {!loading ? (
          <>
            {iconBefore} {label} {iconAfter}
          </>
        ) : (
          <SvgSpinner className="animate-spin fill-gray-300" />
        )}
      </span>
    )
  },
)
