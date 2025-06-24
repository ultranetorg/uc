import { GridSmSvg, ViewStackedSvg } from "assets"
import { memo, useCallback, useState } from "react"
import { twMerge } from "tailwind-merge"

export type ViewType = "grid" | "list"

export type ToggleViewButtonProps = {
  view: ViewType
  onChange(view: ViewType): void
  gridTitle: string
  listTitle: string
}

export const ToggleViewButton = memo(({ view, onChange, gridTitle, listTitle }: ToggleViewButtonProps) => {
  const [viewName, setViewName] = useState(view)

  const handleClick = useCallback(() => {
    const newView = viewName === "grid" ? "list" : "grid"
    setViewName(newView)
    onChange(newView)
  }, [onChange, viewName])

  return (
    <div
      className="transition-base h-10.5 box-border flex w-20 cursor-pointer select-none items-center gap-1 rounded border border-gray-300 bg-gray-100 p-1 hover:bg-gray-200"
      onClick={handleClick}
    >
      <div className={twMerge("rounded-sm p-0.4375", viewName === "grid" && "bg-gray-950")} title={gridTitle}>
        <GridSmSvg className={twMerge("fill-gray-500", viewName === "grid" && "fill-gray-100")} />
      </div>
      <div className={twMerge("rounded-sm p-0.4375", viewName === "list" && "bg-gray-950")} title={listTitle}>
        <ViewStackedSvg className={twMerge("fill-gray-500", viewName === "list" && "fill-gray-100")} />
      </div>
    </div>
  )
})
