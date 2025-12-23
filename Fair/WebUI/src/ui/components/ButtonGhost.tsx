import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type ButtonGhostBaseProps = {
  label: string
  onClick?: () => void
  iconAfter?: JSX.Element
}

export type ButtonGhostProps = PropsWithClassName & ButtonGhostBaseProps

export const ButtonGhost = ({ className, label, onClick, iconAfter }: ButtonGhostProps) => (
  <span
    className={twMerge(
      "disabled:gray-400 flex w-fit cursor-pointer items-center gap-1 rounded text-2xs leading-4 text-gray-800 hover:font-medium",
      className,
    )}
    onClick={onClick}
  >
    {label}
    {iconAfter}
  </span>
)
