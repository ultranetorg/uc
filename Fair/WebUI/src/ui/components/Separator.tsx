import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export const Separator = memo(({ className }: PropsWithClassName) => (
  <hr className={twMerge("h-full w-px border-0 bg-gray-500", className)} />
))
