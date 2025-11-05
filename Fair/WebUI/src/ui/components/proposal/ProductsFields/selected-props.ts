import { ProductFieldCompareViewModel, ProductFieldViewModel } from "types"

export interface SelectedProps<TModel extends ProductFieldViewModel = ProductFieldViewModel> {
  selected?: TModel | null
  onSelect: (node: TModel | null) => void
}

export const isCompareNode = (n?: ProductFieldViewModel | ProductFieldCompareViewModel | null): n is ProductFieldCompareViewModel => {
  return !!n && ("isRemoved" in n || "isAdded" in n || "isChanged" in n)
}

export type CompareStatus = "removed" | "added" | "changed" | null

export const getCompareStatus = (node?: ProductFieldViewModel | ProductFieldCompareViewModel | null): CompareStatus => {
  if (!node) return null
  if (!isCompareNode(node)) return null
  // precedence: removed > added > changed
  if (node.isRemoved) return "removed"
  if (node.isAdded) return "added"
  if (node.isChanged) return "changed"
  return null
}
