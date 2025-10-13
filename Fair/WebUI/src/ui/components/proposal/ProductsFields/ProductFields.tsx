import { useState } from "react"
import { ProposalOption, ProductFieldViewModel } from "types"
import { ProductFieldsTree } from "./ProductFieldsTree.tsx"
import { ProductFieldInfo } from "./ProductFieldInfo.tsx"
import { useTranslation } from "react-i18next"

export type ProductsFieldsTreeProps = {
  proposalOptions: ProposalOption[]
}

export const ProductFields = ({ proposalOptions }: ProductsFieldsTreeProps) => {
  const { t } = useTranslation("productFields")

  const [selected, setSelected] = useState<ProductFieldViewModel |null>(null)

  return (
    <div className="flex max-h-screen gap-6">
      <div className="w-1/4 overflow-auto border-r pr-4">
        <div className="w-fit">
          {proposalOptions.map(option => (
            <ProductFieldsTree
              key={option.operation.productId}
              proposalOption={option}
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
