import { memo, useCallback, useState } from "react"
import { useTranslation } from "react-i18next"

import { ProductFieldViewModel } from "types"
import { SpinnerRowSvg, SvgChevronRightMd } from "assets"

import { SelectedProps } from "../types"
import { getCompareStatus } from "../utils"

export interface ProductFieldsTreeProps extends SelectedProps {
  items: ProductFieldViewModel[]
}

const pretty = (v?: string) => (v ? v.replace(/-/g, " ").replace(/\b\w/g, c => c.toUpperCase()) : "")

const TreeNode = ({
  node,
  onSelect,
  depth = 0,
  selected,
}: {
  node: ProductFieldViewModel
  onSelect: (node: ProductFieldViewModel | null) => void
  depth?: number
  selected: ProductFieldViewModel | null
}) => {
  const hasChildren = !!node.children?.length
  const [closed, setClosed] = useState(depth < 1)
  const select = useCallback(() => {
    if (hasChildren) {
      setClosed(false)
    }
    onSelect(node)
  }, [hasChildren, node, onSelect])

  const status = getCompareStatus(node)

  const rowStatusClass =
    status === "removed"
      ? "opacity-75 line-through text-red-700"
      : status === "added"
        ? "text-green-800"
        : status === "changed"
          ? "text-blue-800"
          : ""

  const selectedClass = selected?.id === node.id ? "bg-gray-100" : ""

  return (
    <>
      <div
        className={`flex cursor-pointer select-none gap-2 p-1 ${rowStatusClass} ${selectedClass}`}
        role="treeitem"
        aria-expanded={hasChildren ? !closed : undefined}
        tabIndex={0}
        aria-label={pretty(node.name)}
      >
        <span className="size-4" onClick={() => setClosed(v => !v)}>
          {hasChildren && (
            <SvgChevronRightMd className={`fill-gray-300 hover:fill-gray-400 ${closed ? "rotate-90" : "rotate-0"}`} />
          )}
        </span>
        <div className={`flex flex-1 gap-2 px-1 hover:underline`} onClick={() => select()}>
          <span className="text-sm">{pretty(node.name)}</span>
        </div>
      </div>
      {hasChildren && !closed && (
        <div className="ml-6 border-l">
          {node.children!.map((child, idx) => (
            <TreeNode
              key={child.id ?? idx}
              node={child}
              selected={selected}
              depth={depth + 1}
              onSelect={onSelect}
            />
          ))}
        </div>
      )}
    </>
  )
}

const MemoTreeNode = memo(TreeNode)

const NoData = (locale: string) => <div className="flex items-center gap-2 text-slate-500">{locale}</div>

export const ProductFieldsTree = memo(({ items, onSelect, selected }: ProductFieldsTreeProps) => {
  const { t } = useTranslation("productFields")

  if (!items?.length) {
    return NoData(t("noData"))
  }

  return (
    <>
      {items.map((node, index) => (
        <MemoTreeNode
          key={node.id ?? index}
          node={node}
          selected={selected ?? null}
          onSelect={onSelect}
        />
      ))}
    </>
  )
})
