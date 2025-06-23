import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export type ButtonOutlineBaseProps = {
  label: string
  onClick?: () => void
}

export type ButtonOutlineProps = PropsWithClassName & ButtonOutlineBaseProps

export const ButtonOutline = ({ className, label, onClick }: ButtonOutlineProps) => (
  <span
    className={twMerge(
      "transition-base w-fit cursor-pointer rounded border border-gray-400 px-4 py-2 text-2sm leading-5 text-gray-800 hover:border-gray-950",
      className,
    )}
    onClick={onClick}
  >
    {label}
  </span>
)
