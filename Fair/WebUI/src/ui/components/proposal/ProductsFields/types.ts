import { ProductFieldViewModel } from "types"

export type CompareStatus = "removed" | "added" | "changed" | null

export interface SelectedProps<TModel extends ProductFieldViewModel = ProductFieldViewModel> {
  selected?: TModel | null
  onSelect: (node: TModel | null) => void
}
