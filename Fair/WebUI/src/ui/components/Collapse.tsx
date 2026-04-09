import { memo, PropsWithChildren, useState } from "react"
import { twMerge } from "tailwind-merge"

import { SvgChevronDownSm } from "assets"
import { PropsWithClassName } from "types"

type CollapseBaseProps = {
  expanded?: boolean
  text: string
}

export type CollapseProps = PropsWithChildren & PropsWithClassName & CollapseBaseProps

export const Collapse = memo(({ children, className, expanded, text }: CollapseProps) => {
  const [isExpanded, setExpanded] = useState<boolean>(expanded ?? false)

  const handleClick = () => setExpanded(prev => !prev)

  return (
    <div className={twMerge("flex gap-1", isExpanded ? "flex-col gap-2" : "items-center", className)}>
      <div className="flex cursor-pointer items-center gap-1" onClick={handleClick}>
        <span className="truncate">{text}</span>{" "}
        <SvgChevronDownSm className={twMerge("fill-gray-800", isExpanded && "rotate-180")} />
      </div>
      {isExpanded && children}
    </div>
  )
})
