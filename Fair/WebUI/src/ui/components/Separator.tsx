import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export const Separator = ({ className }: PropsWithClassName) => (
  <hr className={twMerge("h-full w-px border-0 bg-gray-500", className)} />
)
