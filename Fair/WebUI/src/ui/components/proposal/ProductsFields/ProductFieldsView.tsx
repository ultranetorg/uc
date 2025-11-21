import { memo, ReactElement, useRef, useState, useEffect, useCallback } from "react"
import { UseQueryResult } from "@tanstack/react-query"
import { useTranslation } from "react-i18next"
import { ProductFieldViewModel } from "types"
import { ProductFieldsTree } from "./ProductFieldsTree"
import { SelectedProps } from "./types"

export interface ProductFieldsViewProps extends SelectedProps {
  children?: ReactElement | null
  response: UseQueryResult<ProductFieldViewModel[], Error>
}

export const ProductFieldsView = memo(({ response, selected, onSelect, children }: ProductFieldsViewProps) => {
  const { t } = useTranslation("productFields")

  const containerRef = useRef<HTMLDivElement | null>(null)
  const [leftWidth, setLeftWidth] = useState<number | null>(null) // px
  const draggingRef = useRef(false)

  // Initialize left width to 20% of container on mount
  useEffect(() => {
    const el = containerRef.current
    if (!el) return
    if (leftWidth !== null) return
    const w = el.clientWidth
    setLeftWidth(Math.max(200, Math.round(w * 0.35)))
  }, [leftWidth])

  // Handlers
  const onMouseDown = useCallback(() => {
    draggingRef.current = true
    // prevent text selection while dragging
    document.body.style.userSelect = "none"
  }, [])

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
      setLeftWidth(newLeft)
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
  }, [])

  // Inline styles for left pane and resizer
  const leftStyle: React.CSSProperties = leftWidth ? { width: leftWidth } : { width: "20%" }
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
    <div ref={containerRef} className="flex max-h-screen items-stretch">
      <div className="overflow-auto" style={leftStyle}>
        <div className="w-fit">
          <ProductFieldsTree response={response} selected={selected} onSelect={onSelect} />
        </div>
      </div>

      {/* Resizer */}
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

      <div className="flex-1 overflow-auto">
        {children ?? <div className="p-4 text-gray-400"> {t("selectField")} </div>}
      </div>
    </div>
  )
})
