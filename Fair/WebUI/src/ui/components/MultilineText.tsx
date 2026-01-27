import React, { memo } from "react"

export type MultilineTextProps = {
  children?: string
}

export const MultilineText = memo(({ children }: MultilineTextProps) => {
  const lines = children?.split("\n") ?? []

  return (
    <>
      {lines.map((line, i) => (
        <React.Fragment key={i}>
          {line}
          {i !== lines.length - 1 && <div className="h-2" />}
        </React.Fragment>
      ))}
    </>
  )
})
