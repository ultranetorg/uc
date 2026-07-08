import { RefObject, useEffect, useState } from "react"

export const useIsMultilineText = (ref: RefObject<HTMLElement>, text?: string) => {
  const [isMultiline, setIsMultiline] = useState(false)

  useEffect(() => {
    const el = ref.current
    if (!el) return

    const checkLines = () => {
      const lineHeight = parseFloat(getComputedStyle(el).lineHeight)
      setIsMultiline(el.scrollHeight > lineHeight * 1.5)
    }

    checkLines()

    const observer = new ResizeObserver(checkLines)
    observer.observe(el)
    return () => observer.disconnect()
  }, [ref, text])

  return isMultiline
}
