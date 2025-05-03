import React from "react"
import { memo } from "react"

export type MultilineTextProps = {
  children?: string
}

export const MultilineText = memo(({ children }: MultilineTextProps) => (
  <>
    {children?.split("\n").map((line, i) => (
      <React.Fragment key={i}>
        {line}
        <br />
      </React.Fragment>
    ))}
  </>
))
