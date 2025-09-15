import { DropdownItem } from "ui/components"

export const toDropdownItems = (options?: string[]): DropdownItem[] =>
  (options ?? []).map(x => ({ label: x, value: x.toLowerCase() }))
