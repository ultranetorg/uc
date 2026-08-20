import { memo } from "react"

import { SvgBook, SvgController, SvgFileEarmarkCode, SvgFileEarmarkMusic, SvgFileEarmarkVideo, SvgGridSm } from "assets"
import { ProductType } from "types"
import { FilterButton, FilterButtonProps } from "./FilterButton"

export type FiltersPanelProps = {
  value: ProductType
  onChange: (value: ProductType) => void
}

export const FiltersPanel = memo(({ value, onChange }: FiltersPanelProps) => {
  const items: (Omit<FilterButtonProps, "onClick"> & { type: ProductType })[] = [
    {
      text: "All",
      type: "none",
      icon: SvgGridSm,
    },
    {
      text: "Software",
      type: "software",
      icon: SvgFileEarmarkCode,
    },
    {
      text: "Games",
      type: "game",
      icon: SvgController,
    },
    {
      text: "Video",
      type: "movie",
      icon: SvgFileEarmarkVideo,
    },
    {
      text: "Music",
      type: "music",
      icon: SvgFileEarmarkMusic,
    },
    {
      text: "Books",
      type: "book",
      icon: SvgBook,
    },
  ]

  return (
    <div className="flex gap-3">
      {items.map(x => (
        <FilterButton key={x.type} {...x} checked={value === x.type} onClick={() => onChange(x.type)} />
      ))}
    </div>
  )
})
