import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type ButtonPrimaryBaseProps = {
  iconBefore?: JSX.Element
  iconAfter?: JSX.Element
  label: string
  onClick?: () => void
}

export type ButtonPrimaryProps = PropsWithClassName & ButtonPrimaryBaseProps

export const ButtonPrimary = ({ className, iconBefore, iconAfter, label, onClick }: ButtonPrimaryProps) => (
  <span
    className={twMerge(
      "transition-base flex cursor-pointer select-none items-center justify-center rounded bg-gray-800 px-4 py-3 text-2sm leading-5 text-gray-0 hover:bg-gray-950",
      (iconBefore || iconAfter) && "gap-2",
      className,
    )}
    onClick={onClick}
  >
    {iconBefore} {label} {iconAfter}
  </span>
)
