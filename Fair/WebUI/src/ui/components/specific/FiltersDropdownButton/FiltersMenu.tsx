import { forwardRef, memo } from "react"
import { PropsWithStyle } from "types"

import { DropdownItem, Dropdown } from "ui/components"

import { ResetAllButton } from "./ResetAllButton"

const TEST_ITEMS: DropdownItem[] = [
  { value: "0", label: "Business" },
  { value: "1", label: "Development Tools" },
  { value: "2", label: "Data Science" },
  { value: "3", label: "Graphics and Design" },
  { value: "4", label: "Video" },
]

type FiltersMenuBaseProps = {
  onResetClick(): void
  resetAllLabel: string
}

export type FiltersMenuProps = PropsWithStyle & FiltersMenuBaseProps

export const FiltersMenu = memo(
  forwardRef<HTMLDivElement, FiltersMenuProps>(({ style, onResetClick, resetAllLabel }, ref) => (
    <div
      className="flex flex-col gap-4 rounded-lg border border-[#d9d9d9] bg-gray-50 p-4 shadow-md"
      style={style}
      ref={ref}
    >
      <Dropdown className="w-72" items={TEST_ITEMS} placeholder="Category" />
      <Dropdown className="w-72" items={TEST_ITEMS} placeholder="Author" />
      <Dropdown className="w-72" items={TEST_ITEMS} placeholder="OS" />
      <Dropdown className="w-72" items={TEST_ITEMS} placeholder="Sort" />
      <ResetAllButton onClick={onResetClick} label={resetAllLabel} />
    </div>
  )),
)
