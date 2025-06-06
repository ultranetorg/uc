import { forwardRef, memo } from "react"

import { ChevronDownSvg } from "assets"
import { twMerge } from "tailwind-merge"

export type DropdownButtonProps = {
  expanded?: boolean
}

export const DropdownButton = memo(
  forwardRef<SVGSVGElement, DropdownButtonProps>(({ expanded }, ref) => (
    <ChevronDownSvg ref={ref} className={twMerge("stroke-gray-500", expanded && "rotate-180 transform")} />
  )),
)
