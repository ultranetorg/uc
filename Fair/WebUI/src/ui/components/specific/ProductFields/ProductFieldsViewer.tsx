import { useMemo, useState } from "react"

import { ProductFieldModel, ProductFieldViewModel } from "types"

import { ProductFieldsView } from "../../proposal/ProductsFields/ProductFieldsView"
import { ProductFieldInfo } from "../../proposal/ProductsFields/ProductFieldsTree/ProductFieldInfo"
import { mapFields } from "../../proposal/ProductsFields/utils"

export type ProductFieldsViewerProps = {
  productFields?: ProductFieldModel[] | null
}

export const ProductFieldsViewer = ({ productFields }: ProductFieldsViewerProps) => {
  const [selected, setSelected] = useState<ProductFieldViewModel | null>(null)

  const items = useMemo(() => mapFields(productFields ?? []), [productFields])

  return (
    <ProductFieldsView items={items} selected={selected} onSelect={setSelected}>
      {selected && <ProductFieldInfo node={selected} onSelect={setSelected} />}
    </ProductFieldsView>
  )
}
