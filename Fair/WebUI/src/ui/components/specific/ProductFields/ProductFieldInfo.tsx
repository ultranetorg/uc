import { memo } from "react"
import { useTranslation } from "react-i18next"

import { SvgChevronRight } from "assets"
import { HomeSvg } from "assets/home"
import { kebabToCamel } from "utils"

import { FieldData } from "./FieldData"
import { ProductFieldViewModel, SelectedProps } from "./types"
import { getCompareStatus } from "./utils"

export interface ProductFieldInfoProps extends SelectedProps {
  node: ProductFieldViewModel
}

const BreadcrumbNode = ({ node, onSelect }: ProductFieldInfoProps) => {
  const { t } = useTranslation("productToken")

  return (
    <>
      {node.parent && (
        <>
          <BreadcrumbNode node={node.parent} onSelect={onSelect} />
          <SvgChevronRight className="stroke-gray-400" />
        </>
      )}
      <span
        className="cursor-pointer rounded-sm p-1 text-gray-600 hover:text-gray-900"
        key={node.name}
        onClick={() => onSelect(node)}
      >
        {t(kebabToCamel(node.name))}
      </span>
    </>
  )
}

const FieldBreadcrumbs = ({ node, onSelect }: ProductFieldInfoProps) => {
  return (
    <div className="flex items-center border-b bg-gray-200 px-4 py-1 text-sm">
      <HomeSvg
        className="size-4 cursor-pointer stroke-gray-600 hover:stroke-gray-900"
        onClick={() => onSelect(undefined)}
      />
      <SvgChevronRight className="stroke-gray-400" />
      <BreadcrumbNode node={node} onSelect={onSelect} />
    </div>
  )
}

const FieldsList = ({ node, onSelect }: ProductFieldInfoProps) => {
  return (
    <ul className="divide-y divide-gray-300">
      <li
        key={node.name}
        className="flex cursor-pointer items-center justify-between bg-gray-100 px-4 py-2 text-sm hover:bg-gray-50"
        onClick={() => onSelect(node.parent ?? undefined)}
      >
        ...
      </li>

      {node.children?.map((child, index) => {
        const status = getCompareStatus(node)

        const rowStatusClass =
          status === "removed"
            ? "opacity-75 line-through text-red-700"
            : status === "added"
              ? "text-green-800"
              : status === "changed"
                ? "text-blue-800"
                : ""

        return (
          <li
            key={index}
            className={`flex cursor-pointer items-center justify-between bg-gray-100 px-4 py-2 text-sm hover:bg-gray-50 ${rowStatusClass}`}
            onClick={() => onSelect(child)}
          >
            {child.name}
            <SvgChevronRight className="stroke-gray-400" />
          </li>
        )
      })}
    </ul>
  )
}

export const ProductFieldInfo = memo(({ node, onSelect }: ProductFieldInfoProps) => {
  return (
    <div className="h-full overflow-hidden rounded-md border">
      <FieldBreadcrumbs node={node} onSelect={onSelect} />
      {node.children?.length ? <FieldsList node={node} onSelect={onSelect} /> : <FieldData node={node} />}
    </div>
  )
})
