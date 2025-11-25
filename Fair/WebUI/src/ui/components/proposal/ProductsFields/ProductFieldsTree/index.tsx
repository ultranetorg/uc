import { memo, useCallback, useState } from "react"
import { useTranslation } from "react-i18next"
import { UseQueryResult } from "@tanstack/react-query"

import { ProductFieldViewModel } from "types"
import { SpinnerRowSvg, SvgChevronRightMd } from "assets"

import { SelectedProps } from "../types"
import { getCompareStatus } from "../utils"

export interface ProductFieldsTreeProps<TModel extends ProductFieldViewModel> extends SelectedProps<TModel> {
  response: UseQueryResult<TModel[], Error>
}

const pretty = (v?: string) => (v ? v.replace(/-/g, " ").replace(/\b\w/g, c => c.toUpperCase()) : "")

const TreeNode = <TModel extends ProductFieldViewModel>({
  node,
  onSelect,
  depth = 0,
  selected,
}: {
  node: TModel
  onSelect: (node: TModel | null) => void
  depth?: number
  selected: TModel | null
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
            <SvgChevronRightMd
              className={`fill-gray-300 transition-transform duration-150 hover:fill-gray-400 ${closed ? "rotate-90" : "rotate-0"}`}
            />
          )}
        </span>
        <div className={`flex flex-1 gap-2 px-1 hover:underline`} onClick={() => select()}>
          <span className="text-sm">{pretty(node.name)}</span>
        </div>
      </div>
      {hasChildren && !closed && (
        <div className="ml-6 border-l">
          {node.children!.map((child, idx) => (
            <TreeNode<TModel>
              key={(child as ProductFieldViewModel).id ?? idx}
              node={child as TModel}
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

const MemoTreeNode = memo(TreeNode) as typeof TreeNode

const Loader = (locale: string) => (
  <div className="flex items-center gap-2 text-slate-500">
    <SpinnerRowSvg />
    {locale}
  </div>
)

const NoData = (locale: string) => <div className="flex items-center gap-2 text-slate-500">{locale}</div>
const Error = (locale: string) => <div className="text-red-700">{locale}</div>

export const ProductFieldsTree = memo(
  <TModel extends ProductFieldViewModel>({ response, onSelect, selected }: ProductFieldsTreeProps<TModel>) => {
    const { t } = useTranslation("productFields")
    const { error, data, isPending } = response

    if (isPending) {
      return Loader(t("loading"))
    }

    if (error) {
      return Error(t("loadError"))
    }

    if (!data?.length) {
      return NoData(t("noData"))
    }

    return (
      <>
        {data.map((node, index) => (
          <MemoTreeNode<TModel>
            key={(node as ProductFieldViewModel).id ?? index}
            node={node as TModel}
            selected={selected ?? null}
            onSelect={onSelect}
          />
        ))}
      </>
    )
  },
)
