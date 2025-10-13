import { ProductFieldViewModel } from "types"
import { HomeSvg } from "assets/home.tsx"
import { ProductFieldView } from "./ProductFieldView"
import { memo } from "react"
import { SvgChevronRight } from "../../../../assets"

export type ProductFieldInfoProps = {
  node: ProductFieldViewModel
  onSelect: (node: ProductFieldViewModel) => void
}

const BreadcrumbNode = ({ node, onSelect }: ProductFieldInfoProps) => {
  return (
    <>
      {node.parent && (
        <>
          <BreadcrumbNode node={node.parent} onSelect={onSelect} />
          <SvgChevronRight className="stroke-gray-400" />
        </>
      )}
      <span
        className="cursor-pointer rounded-sm px-1 py-1 text-gray-600 hover:text-gray-900"
        key={node.name}
        onClick={() => onSelect(node)}
      >
        {node.name}
      </span>
    </>
  )
}

const FieldBreadcrumbs = ({ node, onSelect }: ProductFieldInfoProps) => {
  return (
    <div className="flex items-center gap-2 border-b bg-gray-200 px-4 py-1 text-sm">
      <HomeSvg className="h-4 w-4 cursor-pointer stroke-gray-600 hover:stroke-gray-900" />
      <SvgChevronRight className="stroke-gray-400" />
      <BreadcrumbNode node={node} onSelect={onSelect} />
    </div>
  )
}

const FieldsList = ({ node, onSelect }: ProductFieldInfoProps) => {
  return (
    <ul className="divide-y divide-gray-300">
      {node.parent ? (
        <li
          key={node.name}
          className="flex cursor-pointer items-center justify-between px-4 py-2 text-sm hover:bg-gray-100"
          onClick={() => onSelect(node.parent!)}
        >
          ...
        </li>
      ) : (
        <></>
      )}

      {node.children?.map((child, index) => (
        <li
          key={index}
          className="flex cursor-pointer items-center justify-between px-4 py-2 text-sm hover:bg-gray-100"
          onClick={() => onSelect(child)}
        >
          {child.name}
        </li>
      ))}
    </ul>
  )
}

export const ProductFieldInfo = memo(({ node, onSelect }: ProductFieldInfoProps) => {
  return (
    <div className="overflow-hidden rounded-md border">
      <FieldBreadcrumbs node={node} onSelect={onSelect} />
      {node.children?.length ? <FieldsList node={node} onSelect={onSelect} /> : <ProductFieldView node={node} />}
    </div>
  )
})
