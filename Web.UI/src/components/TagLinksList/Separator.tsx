import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export const Separator = memo(({ className }: PropsWithClassName) => (
  <div className={twMerge("h-4 border-r border-dark-blue-100", className)} />
))
