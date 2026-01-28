import { forwardRef, memo } from "react"
import { twMerge } from "tailwind-merge"

import { ChevronDownSvg } from "assets"

export type DropdownButtonProps = {
  expanded?: boolean
}

export const DropdownButton = memo(
  forwardRef<SVGSVGElement, DropdownButtonProps>(({ expanded }, ref) => (
    <ChevronDownSvg ref={ref} className={twMerge("stroke-gray-500", expanded && "rotate-180 transform")} />
  )),
)
