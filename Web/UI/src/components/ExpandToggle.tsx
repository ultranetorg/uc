import { memo, useCallback, useState } from "react"

import { SvgArrowDownSm, SvgArrowUpSm } from "assets"

export type ExpandToggleProps = {
  expandLabel?: string
  collapseLabel?: string
  onToggle?: (expanded: boolean) => void
}

export const ExpandToggle = memo((props: ExpandToggleProps) => {
  const { expandLabel = "Show more", collapseLabel = "Show less", onToggle } = props

  const [expanded, setExpanded] = useState(false)

  const handleClick = useCallback(() => {
    const value = !expanded
    setExpanded(value)
    onToggle?.(value) // && onToggle(value)
  }, [expanded, onToggle])

  return (
    <div className="flex cursor-pointer items-center gap-2 text-xs text-cyan-500" onClick={handleClick}>
      {!expanded ? (
        <>
          {expandLabel}
          <SvgArrowDownSm className="stroke-cyan-500" />
        </>
      ) : (
        <>
          {collapseLabel}
          <SvgArrowUpSm className="stroke-cyan-500" />
        </>
      )}
    </div>
  )
})
