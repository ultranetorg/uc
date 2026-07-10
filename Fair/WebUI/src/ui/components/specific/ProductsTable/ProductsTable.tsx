import { memo } from "react"
import { useTranslation } from "react-i18next"

import { ProductAuthor, PropsWithClassName } from "types"

import { ProductTableRow } from "./ProductTableRow"

export type ProductsTableBaseProps = {
  items: ProductAuthor[]
  onProductStoresClick: (id: string) => void
}

export type ProductsTableProps = PropsWithClassName & ProductsTableBaseProps

export const ProductsTable = memo(({ items, onProductStoresClick }: ProductsTableProps) => {
  const { t } = useTranslation("profile")

  return (
    <div className="flex flex-col rounded-lg border border-gray-300 bg-gray-100">
      <div className="flex justify-between bg-gray-200 px-4 py-2 text-2xs font-medium leading-4">
        <span className="w-[43%] capitalize">{t("common:product")}</span>
        <span className="w-[27%] text-center">{t("totalPublications")}</span>
      </div>
      <div className="divide-y divide-gray-300">
        {items.map(x => (
          <ProductTableRow key={x.id} {...x} onProductStoresClick={onProductStoresClick} />
        ))}
      </div>
    </div>
  )
})
