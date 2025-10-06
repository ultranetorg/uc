import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type ButtonPrimaryBaseProps = {
  iconBefore?: JSX.Element
  iconAfter?: JSX.Element
  label: string
  onClick?: () => void
}

export type ButtonPrimaryProps = PropsWithClassName &
  Partial<Pick<HTMLButtonElement, "disabled" | "type">> &
  ButtonPrimaryBaseProps

export const ButtonPrimary = ({
  className,
  disabled,
  iconBefore,
  iconAfter,
  label,
  type,
  onClick,
}: ButtonPrimaryProps) => (
  <button
    className={twMerge(
      "flex cursor-pointer select-none items-center justify-center rounded bg-gray-800 px-4 py-3 text-2sm leading-5 text-gray-0 hover:bg-gray-950",
      disabled && "cursor-not-allowed bg-gray-400 hover:bg-gray-400",
      (iconBefore || iconAfter) && "gap-2",
      className,
    )}
    onClick={onClick}
    type={type}
    disabled={disabled}
  >
    {iconBefore} {label} {iconAfter}
  </button>
)
