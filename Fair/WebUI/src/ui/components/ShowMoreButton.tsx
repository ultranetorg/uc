import { twMerge } from "tailwind-merge"

import { SvgChevronDownSm } from "assets"
import { PropsWithClassName } from "types"

export type ShowMoreButtonBaseProps = {
  isExpanded: boolean
  onExpand(expanded: boolean): void
  showLessLabel: string
  showMoreLabel: string
}

export type ShowMoreButtonProps = PropsWithClassName & ShowMoreButtonBaseProps

export const ShowMoreButton = ({
  className,
  isExpanded,
  onExpand,
  showLessLabel,
  showMoreLabel,
}: ShowMoreButtonProps) => {
  return (
    <div className={twMerge("flex cursor-pointer items-center", className)} onClick={() => onExpand(!isExpanded)}>
      <span>{isExpanded ? showLessLabel : showMoreLabel}</span>
      <SvgChevronDownSm className={twMerge("fill-gray-800", isExpanded && "rotate-180")} />
    </div>
  )
}
