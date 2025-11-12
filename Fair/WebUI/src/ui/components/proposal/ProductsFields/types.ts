import { ProductFieldBase, ProductFieldViewModel } from "types"

export type CompareStatus = "removed" | "added" | "changed" | null

export interface SelectedProps<TModel extends ProductFieldViewModel = ProductFieldViewModel> {
  selected?: TModel | null
  onSelect: (node: TModel | null) => void
}

export interface ProductFieldCompareViewModel extends ProductFieldBase<ProductFieldCompareViewModel> {
  id: string
  parent?: ProductFieldCompareViewModel
  isRemoved?: true
  isAdded?: true
  isChanged?: true
  oldValue?: string
}
