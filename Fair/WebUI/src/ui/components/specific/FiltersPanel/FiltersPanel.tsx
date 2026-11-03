import { memo } from "react"

import {
  SvgBookSm,
  SvgControllerSm,
  SvgFileEarmarkCodeSm,
  SvgFileEarmarkMusicSm,
  SvgFileEarmarkVideoSm,
  SvgGridSm,
} from "assets"
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
      icon: SvgFileEarmarkCodeSm,
      iconColor: "stroke",
    },
    {
      text: "Games",
      type: "game",
      icon: SvgControllerSm,
      iconColor: "stroke",
    },
    {
      text: "Video",
      type: "movie",
      icon: SvgFileEarmarkVideoSm,
      iconColor: "stroke",
    },
    {
      text: "Music",
      type: "music",
      icon: SvgFileEarmarkMusicSm,
      iconColor: "stroke",
    },
    {
      text: "Books",
      type: "book",
      icon: SvgBookSm,
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
