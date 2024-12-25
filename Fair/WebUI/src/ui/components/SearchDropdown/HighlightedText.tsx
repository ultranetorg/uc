import { Fragment, useMemo } from "react"

type HighlightedTextProps = {
  text: string
  textToHighlight: string
}

export const HighlightedText = ({ text, textToHighlight }: HighlightedTextProps) => {
  const highlighted = useMemo(() => {
    const parts = text.split(new RegExp(`(${textToHighlight})`, "i"))
    return parts.map((part, index) => (
      <Fragment key={index}>
        {part.toLowerCase() === textToHighlight.toLowerCase() ? (
          <b style={{ backgroundColor: "#3899BE" }}>{part}</b>
        ) : (
          part
        )}
      </Fragment>
    ))
  }, [text, textToHighlight])

  return <span>{highlighted}</span>
}
