import { memo, PropsWithChildren } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type BadgeVariant = "default" | "warning"

const VARIANT_CLASSNAMES: Record<BadgeVariant, string> = {
  default: "bg-gray-200",
  warning: "bg-warning",
}

export type BadgeProps = PropsWithChildren<PropsWithClassName> & {
  variant?: BadgeVariant
}

export const Badge = memo(({ variant = "default", className, children }: BadgeProps) => (
  <span className={twMerge("w-fit shrink-0 rounded px-2 py-0.5 text-2xs", VARIANT_CLASSNAMES[variant], className)}>
    {children}
  </span>
))
