import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type ButtonGhostBaseProps = {
  label: string
  onClick?: () => void
}

export type ButtonGhostProps = PropsWithClassName & ButtonGhostBaseProps

export const ButtonGhost = ({ className, label, onClick }: ButtonGhostProps) => (
  <span
    className={twMerge(
      "disabled:gray-400 w-fit cursor-pointer rounded text-2xs leading-4 text-gray-800 hover:font-medium",
      className,
    )}
    onClick={onClick}
  >
    {label}
  </span>
)
