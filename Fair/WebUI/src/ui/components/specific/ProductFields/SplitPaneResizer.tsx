import { memo, RefObject, useEffect, useRef } from "react"

export type SplitPaneResizerProps = {
  containerRef: RefObject<HTMLDivElement>
  leftWidth: number | undefined
  onLeftWidthChange: (width: number) => void
}

export const SplitPaneResizer = memo(({ containerRef, onLeftWidthChange }: SplitPaneResizerProps) => {
  const draggingRef = useRef(false)

  const onMouseDown = () => {
    draggingRef.current = true
    document.body.style.userSelect = "none"
  }

  useEffect(() => {
    const handleMove = (e: MouseEvent) => {
      if (!draggingRef.current) return
      const el = containerRef.current
      if (!el) return
      const rect = el.getBoundingClientRect()
      const min = 160 // min left pane px
      const max = rect.width - 160 // leave at least 160px for right pane
      let newLeft = Math.round(e.clientX - rect.left)
      if (newLeft < min) newLeft = min
      if (newLeft > max) newLeft = max
      onLeftWidthChange(newLeft)
    }

    const handleUp = () => {
      if (!draggingRef.current) return
      draggingRef.current = false
      document.body.style.userSelect = ""
    }

    window.addEventListener("mousemove", handleMove)
    window.addEventListener("mouseup", handleUp)

    return () => {
      window.removeEventListener("mousemove", handleMove)
      window.removeEventListener("mouseup", handleUp)
      document.body.style.userSelect = ""
    }
  }, [containerRef, onLeftWidthChange])

  const resizerStyle: React.CSSProperties = {
    width: 9,
    cursor: "col-resize",
    background: "transparent",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    userSelect: "none",
  }

  return (
    <div
      role="separator"
      aria-orientation="vertical"
      onMouseDown={onMouseDown}
      className="relative m-1 flex items-center rounded-full border-2 border-none hover:border-dashed"
      style={resizerStyle}
      title="Drag to resize"
    >
      <div>
        <div className="flex h-12 flex-col items-center justify-center gap-1">
          <div className="size-1 rounded-full bg-gray-200" />
          <div className="size-1 rounded-full bg-gray-300" />
          <div className="size-1 rounded-full bg-gray-200" />
        </div>
      </div>
    </div>
  )
})
