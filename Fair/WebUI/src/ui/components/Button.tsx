import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type ButtonBaseProps = {
  label?: string
  onClick?: () => void
}

type ButtonProps = PropsWithClassName<ButtonBaseProps>

export const Button = ({ className, label, onClick }: ButtonProps) => {
  return (
    <div
      className={twMerge(
        "cursor-pointer select-none rounded-md bg-[#132C38] px-6 py-4 hover:bg-[#1F485C] active:bg-[#0E2029]",
        className,
      )}
      onClick={onClick}
    >
      {label}
    </div>
  )
}
