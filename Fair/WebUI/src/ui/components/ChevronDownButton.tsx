import { forwardRef, memo } from "react"
import { twMerge } from "tailwind-merge"

import { ChevronDownSvg } from "assets"

export type ChevronDownButtonProps = {
  expanded?: boolean
}

export const ChevronDownButton = memo(
  forwardRef<SVGSVGElement, ChevronDownButtonProps>(({ expanded }, ref) => (
    <ChevronDownSvg ref={ref} className={twMerge("stroke-gray-500", expanded && "rotate-180 transform")} />
  )),
)
