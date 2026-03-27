import { memo, useEffect, useMemo, useState } from "react"

import { FieldValue } from "types"

import { ProductFields } from "./ProductFields"
import { ProductFieldInfo } from "./ProductFieldInfo"
import { ProductFieldViewModel } from "./types"
import { mapFields } from "./utils"

export type ProductFieldsTreeProps = {
  productFields?: FieldValue[]
}

export const ProductFieldsTree = memo(({ productFields }: ProductFieldsTreeProps) => {
  const [selected, setSelected] = useState<ProductFieldViewModel | undefined>()

  const items = useMemo(() => mapFields(productFields ?? []), [productFields])

  useEffect(() => {
    if (productFields != null && productFields.length > 0) {
      setSelected(items[0])
    }
  }, [items, productFields])

  return (
    <ProductFields items={items} selected={selected} onSelect={setSelected}>
      {selected && <ProductFieldInfo node={selected} onSelect={setSelected} />}
    </ProductFields>
  )
})
