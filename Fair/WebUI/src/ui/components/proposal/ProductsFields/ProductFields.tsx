import { useState } from "react"
import { ProductFieldViewModel } from "types"
import { ProductFieldsTree } from "./ProductFieldsTree.tsx"
import { ProductFieldInfo } from "./ProductFieldInfo.tsx"
import { useTranslation } from "react-i18next"

export type ProductsFieldsTreeProps = {
  productIds: string[]
}

export const ProductFields = ({ productIds }: ProductsFieldsTreeProps) => {
  const { t } = useTranslation("productFields")

  const [selected, setSelected] = useState<ProductFieldViewModel | null>(null)

  return (
    <div className="flex max-h-screen gap-6">
      <div className="w-1/5 overflow-auto border-r pr-4">
        <div className="w-fit">
          {productIds.map(productId => (
            <ProductFieldsTree
              key={productId}
              productId={productId}
              selected={selected}
              onSelect={node => setSelected(node)}
            />
          ))}
        </div>
      </div>
      <div className="flex-1">
        {selected ? (
          <ProductFieldInfo node={selected} onSelect={node => setSelected(node)} />
        ) : (
          <div className="p-4 text-gray-400"> {t("selectField")} </div>
        )}
      </div>
    </div>
  )
}
