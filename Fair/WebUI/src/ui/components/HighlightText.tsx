import { memo } from "react"

type HighlightTextBaseProps = {
  children: string
  className: string
  highlightText: string
}

export type HighlightTextProps = HighlightTextBaseProps

export const HighlightText = memo(({ children, className, highlightText }: HighlightTextProps) => {
  if (!highlightText) {
    return <>{children}</>
  }

  const parts = children.split(new RegExp(`(${highlightText})`, "gi"))

  return (
    <>
      {parts.map((part, index) =>
        part.toLowerCase() === highlightText.toLowerCase() ? (
          <span key={index} className={className}>
            {part}
          </span>
        ) : (
          part
        ),
      )}
    </>
  )
})
