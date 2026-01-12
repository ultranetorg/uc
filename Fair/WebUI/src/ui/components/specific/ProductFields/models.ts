import { ProductFieldModel } from "types"

export type CompareStatus = "removed" | "added" | "changed" | undefined

export type ProductFieldViewModel = ProductFieldModel & {
  id: string
  parent?: ProductFieldViewModel
  children?: ProductFieldViewModel[]
}

export type ProductFieldCompareViewModel = ProductFieldModel & {
  id: string
  parent?: ProductFieldCompareViewModel
  children?: ProductFieldCompareViewModel[]
  isRemoved?: true
  isAdded?: true
  isChanged?: true
  oldValue?: unknown
}

export interface SelectedProps<TModel extends ProductFieldViewModel = ProductFieldViewModel> {
  selected?: TModel
  onSelect: (node: TModel | undefined) => void
}
