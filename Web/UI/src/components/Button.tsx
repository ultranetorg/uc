import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type ButtonBaseProps = {
  label: string
  disabled?: boolean
  onClick?: () => void
}

export type ButtonProps = PropsWithClassName & ButtonBaseProps

export const Button = memo(({ className, label, disabled = false, onClick }: ButtonProps) => (
  <button
    className={twMerge(
      "box-border h-10 cursor-pointer select-none rounded-lg border-2 border-transparent bg-cyan-700 px-3 leading-[17px] text-gray-200 shadow-none outline-none hover:bg-cyan-600 focus:border-2 focus:border-cyan-500 focus:outline-none active:bg-cyan-700 disabled:cursor-default disabled:bg-cyan-950 disabled:text-gray-500",
      className,
    )}
    onClick={onClick}
    disabled={disabled}
  >
    {label}
  </button>
))
