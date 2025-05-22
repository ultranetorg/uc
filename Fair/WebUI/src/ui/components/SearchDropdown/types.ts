import { MouseEvent } from "react"

export type SearchDropdownItem = {
  value: string
  label: string
}

export type IndicatorsContainerSelectProps = {
  onClearInputClick?: (e: MouseEvent<HTMLDivElement>) => void
  onSearchClick?: (e: MouseEvent<HTMLDivElement>) => void
}

export type MenuSelectProps = {
  noticeMessage?: string
}
