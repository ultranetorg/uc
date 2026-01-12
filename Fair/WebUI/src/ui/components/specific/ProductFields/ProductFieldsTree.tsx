import { memo, useMemo, useState } from "react"

import { ProductField } from "types"

import { ProductFields } from "./ProductFields"
import { ProductFieldInfo } from "./ProductFieldInfo"
import { ProductFieldViewModel } from "./types"
import { mapFields } from "./utils"

export type ProductFieldsTreeProps = {
  productFields?: ProductField[]
}

export const ProductFieldsTree = memo(({ productFields }: ProductFieldsTreeProps) => {
  const [selected, setSelected] = useState<ProductFieldViewModel | undefined>()

  const items = useMemo(() => mapFields(productFields ?? []), [productFields])

  return (
    <ProductFields items={items} selected={selected} onSelect={setSelected}>
      {selected && <ProductFieldInfo node={selected} onSelect={setSelected} />}
    </ProductFields>
  )
})
