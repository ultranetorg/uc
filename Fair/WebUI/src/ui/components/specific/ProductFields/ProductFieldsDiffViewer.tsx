import { memo, useMemo, useState } from "react"

import { ProductFieldModel } from "types"

import { ProductFields } from "./ProductFields"
import { ProductFieldInfo } from "./ProductFieldInfo"
import { mergeFields } from "./utils"
import { ProductFieldViewModel } from "./models"

export type ProductFieldsDiffProps = {
  productFieldsFrom?: ProductFieldModel[]
  productFieldsTo?: ProductFieldModel[]
}

export const ProductFieldsDiff = memo(({ productFieldsFrom, productFieldsTo }: ProductFieldsDiffProps) => {
  const [selected, setSelected] = useState<ProductFieldViewModel | undefined>(undefined)

  const items = useMemo(
    () => mergeFields({ from: productFieldsFrom ?? [], to: productFieldsTo ?? [] }),
    [productFieldsFrom, productFieldsTo],
  )

  return (
    <ProductFields items={items} selected={selected} onSelect={setSelected}>
      {selected && <ProductFieldInfo node={selected} onSelect={setSelected} />}
    </ProductFields>
  )
})
