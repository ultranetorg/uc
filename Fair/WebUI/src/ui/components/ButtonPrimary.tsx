import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type ButtonPrimaryBaseProps = {
  icon?: JSX.Element
  label: string
  onClick?: () => void
}

export type ButtonPrimaryProps = PropsWithClassName & ButtonPrimaryBaseProps

export const ButtonPrimary = ({ className, icon, label, onClick }: ButtonPrimaryProps) => (
  <span
    className={twMerge(
      "transition-base flex cursor-pointer items-center justify-center rounded bg-gray-800 px-4 py-3 text-2sm leading-5 text-gray-0 hover:bg-gray-950",
      icon && "gap-2",
      className,
    )}
    onClick={onClick}
  >
    {icon} {label}
  </span>
)
