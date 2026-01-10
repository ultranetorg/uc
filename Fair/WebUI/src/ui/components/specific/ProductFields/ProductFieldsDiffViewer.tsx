import { useMemo, useState } from "react"

import { ProductFieldModel, ProductFieldViewModel } from "types"

import { ProductFieldsView } from "../../proposal/ProductsFields/ProductFieldsView"
import { ProductFieldInfo } from "../../proposal/ProductsFields/ProductFieldsTree/ProductFieldInfo"
import { mergeFields } from "../../proposal/ProductsFields/utils"

export type ProductFieldsDiffViewerProps = {
  productFieldsFrom?: ProductFieldModel[] | null
  productFieldsTo?: ProductFieldModel[] | null
}

export const ProductFieldsDiffViewer = ({
  productFieldsFrom,
  productFieldsTo,
}: ProductFieldsDiffViewerProps) => {
  const [selected, setSelected] = useState<ProductFieldViewModel | null>(null)

  const items = useMemo(
    () => mergeFields({ from: productFieldsFrom ?? [], to: productFieldsTo ?? [] }),
    [productFieldsFrom, productFieldsTo],
  )

  return (
    <ProductFieldsView items={items} selected={selected} onSelect={setSelected}>
      {selected && <ProductFieldInfo node={selected} onSelect={setSelected} />}
    </ProductFieldsView>
  )
}
