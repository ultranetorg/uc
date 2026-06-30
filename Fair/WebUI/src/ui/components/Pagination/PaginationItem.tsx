import { memo, PropsWithChildren } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type PaginationItemBaseProps = {
  active?: boolean
  disabled?: boolean
  onClick?: () => void
}

type PaginationItemProps = PropsWithChildren & PropsWithClassName & PaginationItemBaseProps

export const PaginationItem = memo(({ children, className, active, disabled, onClick }: PaginationItemProps) => (
  <div
    className={twMerge(
      "flex h-9 w-9 select-none items-center justify-center rounded stroke-gray-950 text-sm font-medium leading-4.25 text-gray-900 hover:bg-dark-100/10",
      active === true && "bg-gray-950 font-bold text-zinc-100 hover:bg-gray-950 hover:text-zinc-100",
      disabled ? "opacity-40 hover:bg-transparent" : "cursor-pointer",
      className,
    )}
    onClick={disabled ? undefined : onClick}
  >
    {children}
  </div>
))
