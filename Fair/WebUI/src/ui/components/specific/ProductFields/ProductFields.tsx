import { memo, ReactElement, useCallback, useEffect, useRef, useState } from "react"
import { useTranslation } from "react-i18next"

import { FieldsTree } from "./FieldsTree"
import { SplitPaneResizer } from "./SplitPaneResizer"
import { SelectedProps, ProductFieldViewModel } from "./types"

export interface ProductFieldsProps extends SelectedProps {
  children?: ReactElement | null
  items: ProductFieldViewModel[]
}

export const ProductFields = memo(({ items, selected, onSelect, children }: ProductFieldsProps) => {
  const { t } = useTranslation("productFields")

  const containerRef = useRef<HTMLDivElement | null>(null)
  const [leftWidth, setLeftWidth] = useState<number | undefined>()

  // Initialize left width to 35% of container on mount
  useEffect(() => {
    const el = containerRef.current
    if (!el) return
    if (leftWidth !== undefined) return
    const w = el.clientWidth
    setLeftWidth(Math.max(200, Math.round(w * 0.35)))
  }, [leftWidth])

  const handleWidthChange = useCallback((width: number) => {
    setLeftWidth(width)
  }, [])

  return (
    <div>
      <div ref={containerRef} className="flex max-h-screen items-stretch">
        <div className="overflow-auto" style={leftWidth ? { width: leftWidth } : { width: "20%" }}>
          <div className="w-fit">
            <FieldsTree items={items} selected={selected} onSelect={onSelect} />
          </div>
        </div>

        <SplitPaneResizer containerRef={containerRef} leftWidth={leftWidth} onLeftWidthChange={handleWidthChange} />

        <div className="flex-1 overflow-auto">
          {children ?? <div className="p-4 text-gray-400"> {t("selectField")} </div>}
        </div>
      </div>
    </div>
  )
})
