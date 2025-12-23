import { memo, MouseEvent } from "react"
import { twMerge } from "tailwind-merge"

import { SvgSpinner } from "assets"
import { PropsWithClassName } from "types"

type ButtonPrimaryBaseProps = {
  iconBefore?: JSX.Element
  iconAfter?: JSX.Element
  label: string
  loading?: boolean
  onClick?: (e: MouseEvent<HTMLButtonElement>) => void
}

export type ButtonPrimaryProps = Partial<Pick<HTMLButtonElement, "disabled" | "type">> &
  PropsWithClassName &
  ButtonPrimaryBaseProps

export const ButtonPrimary = memo(
  ({
    disabled = false,
    type = "button",
    className,
    iconBefore,
    iconAfter,
    label,
    loading = false,
    onClick,
  }: ButtonPrimaryProps) => {
    const handleClick = (e: MouseEvent<HTMLButtonElement>) => {
      if (loading) {
        e.preventDefault()
        return
      }
      onClick?.(e)
    }

    return (
      <button
        className={twMerge(
          "flex cursor-pointer select-none items-center justify-center rounded bg-gray-800 px-4 py-3 text-2sm leading-5 text-gray-0 hover:bg-gray-950",
          loading && "cursor-default hover:bg-gray-800",
          disabled && "cursor-not-allowed bg-gray-400 hover:bg-gray-400",
          (iconBefore || iconAfter) && "gap-2",
          className,
        )}
        onClick={handleClick}
        type={type}
        disabled={disabled}
      >
        {!loading ? (
          <>
            {iconBefore} {label} {iconAfter}
          </>
        ) : (
          <SvgSpinner className="animate-spin fill-gray-500" />
        )}
      </button>
    )
  },
)
