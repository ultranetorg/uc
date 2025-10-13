import { useCallback, useMemo, useState, memo } from "react"
import { ProposalOption, ProductFieldModel, ProductFieldViewModel } from "types"
import { useGetProductFields } from "entities"
import { SpinnerRowSvg, SvgChevronRightMd } from "assets"
import { useTranslation } from "react-i18next"

export interface ProductFieldsTreeProps {
  proposalOption: ProposalOption
  onSelect: (node: ProductFieldViewModel) => void
  selected?: ProductFieldViewModel | null
}

/**
 * Преобразует строку в красивый вид (например, для отображения имени поля)
 */
const pretty = (v?: string) => (v ? v.replace(/-/g, " ").replace(/\b\w/g, c => c.toUpperCase()) : "")

/**
 * Рекурсивно добавляет parent в каждый элемент дерева
 */
const mapItems = (
  items: ProductFieldModel[],
  parent: ProductFieldViewModel | undefined = undefined,
  index: number = 0,
): ProductFieldViewModel[] => {
  return items.map(item => {
    const newItem: ProductFieldViewModel = { ...item, parent, children: undefined, id: `${item.name}_${++index}` }
    if (item.children && item.children.length > 0) {
      newItem.children = mapItems(item.children, newItem, index)
    }
    return newItem
  })
}

const TreeNode = memo(
  ({
    node,
    onSelect,
    depth = 0,
    selected,
  }: {
    node: ProductFieldViewModel
    onSelect: (node: ProductFieldViewModel) => void
    depth?: number
    selected?: ProductFieldViewModel | null
  }) => {
    const hasChildren = !!node.children?.length
    const [closed, setClosed] = useState(depth < 1)
    const select = useCallback(() => {
      if (hasChildren) {
        setClosed(false)
      }
      onSelect(node)
    }, [hasChildren, node, onSelect])

    return (
      <>
        <div
          className="flex cursor-pointer select-none gap-2 py-1"
          role="treeitem"
          aria-expanded={hasChildren ? !closed : undefined}
          tabIndex={0}
          aria-label={pretty(node.name)}
        >
          <span className="h-4 w-4" onClick={() => setClosed(v => !v)}>
            {hasChildren && (
              <SvgChevronRightMd className={`fill-gray-300 hover:fill-gray-400 ${closed ? "rotate-90" : "rotate-0"}`} />
            )}
          </span>
          <div
            className={`flex gap-2 px-1 hover:underline ${selected?.id === node.id ? "bg-gray-100" : ""}`}
            onClick={select}
          >
            <span className="text-sm">{pretty(node.name)}</span>
          </div>
        </div>
        {hasChildren && !closed && (
          <div className="ml-6 border-l">
            {node.children!.map((child, index) => (
              <TreeNode key={index} node={child} selected={selected} depth={depth + 1} onSelect={onSelect} />
            ))}
          </div>
        )}
      </>
    )
  },
)
TreeNode.displayName = "TreeNode"

const Loader = (locale: string) => (
  <div className="flex items-center gap-2 text-slate-500">
    <SpinnerRowSvg />
    {locale}
  </div>
)

const NoData = (locale: string) => <div className="flex items-center gap-2 text-slate-500">{locale}</div>
const Error = (locale: string) => <div className="text-red-700">{locale}</div>

export const ProductFieldsTree = ({ proposalOption, onSelect, selected }: ProductFieldsTreeProps) => {
  const { t } = useTranslation("productFields")
  const productId = proposalOption.operation.productId
  const { error, data, isPending } = useGetProductFields(productId)

  const roots = useMemo(() => (data?.items ? mapItems(data.items) : []), [data])

  if (isPending) {
    return Loader(t("loading"))
  }

  if (error) {
    return Error(t("loadError"))
  }

  if (!roots.length) {
    return NoData(t("noData"))
  }

  return (
    <>
      {roots.map((root, index) => (
        <TreeNode key={index} node={root} selected={selected} onSelect={onSelect} />
      ))}
    </>
  )
}
