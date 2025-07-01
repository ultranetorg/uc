import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export type ButtonOutlineBaseProps = {
  disabled?: boolean
  label: string
  onClick?: () => void
}

export type ButtonOutlineProps = PropsWithClassName & ButtonOutlineBaseProps

export const ButtonOutline = ({ className, disabled, label, onClick }: ButtonOutlineProps) => (
  <span
    className={twMerge(
      "flex w-fit select-none items-center justify-center rounded border px-4 py-2 text-2sm leading-5",
      className,
      disabled !== true
        ? "transition-base cursor-pointer border-gray-400 text-gray-800 hover:border-gray-950"
        : "cursor-default border-gray-200 text-gray-400",
    )}
    onClick={disabled !== true ? onClick : undefined}
  >
    {label}
  </span>
)
