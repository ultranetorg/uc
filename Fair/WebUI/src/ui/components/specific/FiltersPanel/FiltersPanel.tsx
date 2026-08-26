import { memo } from "react"

import { SvgBook, SvgController, SvgFileEarmarkCode, SvgFileEarmarkMusic, SvgFileEarmarkVideo, SvgGrid } from "assets"
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
      icon: SvgGrid,
    },
    {
      text: "Software",
      type: "software",
      icon: SvgFileEarmarkCode,
      iconColor: "stroke",
    },
    {
      text: "Games",
      type: "game",
      icon: SvgController,
      iconColor: "stroke",
    },
    {
      text: "Video",
      type: "movie",
      icon: SvgFileEarmarkVideo,
      iconColor: "stroke",
    },
    {
      text: "Music",
      type: "music",
      icon: SvgFileEarmarkMusic,
      iconColor: "stroke",
    },
    {
      text: "Books",
      type: "book",
      icon: SvgBook,
      iconColor: "stroke",
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
