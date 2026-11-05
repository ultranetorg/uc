import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"
import { SvgSpinnerXl } from "assets"

export const Spinner = memo(({ className }: PropsWithClassName) => (
  <div className={twMerge("flex w-full items-center justify-center py-8", className)} role="status" aria-busy="true">
    <SvgSpinnerXl className="animate-spin fill-gray-300" />
  </div>
))
