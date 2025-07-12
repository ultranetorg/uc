import { useRef, useState, useEffect } from "react"
import { twMerge } from "tailwind-merge"

import { ShowMoreButton } from "ui/components"
import { PropsWithClassName } from "types"

type ExpandableTextBaseProps = {
  maxLines?: number
  text: string
  readLessLabel: string
  readMoreLabel: string
}

export type ExpandableTextProps = PropsWithClassName & ExpandableTextBaseProps

export const ExpandableText = ({
  maxLines = 3,
  className,
  text,
  readLessLabel,
  readMoreLabel,
}: ExpandableTextProps) => {
  const [isExpanded, setExpanded] = useState<boolean>(false)
  const [showToggle, setShowToggle] = useState<boolean>(false)
  const textRef = useRef<HTMLDivElement | null>(null)

  useEffect(() => {
    const el = textRef.current
    if (el) {
      const lineHeight = parseFloat(getComputedStyle(el).lineHeight || "0")
      const maxHeight = lineHeight * maxLines

      if (el.scrollHeight > maxHeight + 1) {
        setShowToggle(true)
      }
    }
  }, [maxLines, text])

  return (
    <div className={twMerge("flex w-full flex-col gap-2", className)}>
      <div ref={textRef} className={`${!isExpanded && "line-clamp-3"}`}>
        {text}
      </div>
      {showToggle && (
        <ShowMoreButton
          isExpanded={isExpanded}
          onExpand={setExpanded}
          showLessLabel={readLessLabel}
          showMoreLabel={readMoreLabel}
        />
      )}
    </div>
  )
}
