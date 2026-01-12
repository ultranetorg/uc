import { memo, useMemo, useState } from "react"

import { ProductFieldModel } from "types"

import { ProductFields } from "./ProductFields"
import { ProductFieldInfo } from "./ProductFieldInfo"
import { mapFields } from "./utils"
import { ProductFieldViewModel } from "./models"

export type ProductFieldsTreeProps = {
  productFields?: ProductFieldModel[]
}

export const ProductFieldsTree = memo(({ productFields }: ProductFieldsTreeProps) => {
  const [selected, setSelected] = useState<ProductFieldViewModel | undefined>(undefined)

  const items = useMemo(() => mapFields(productFields ?? []), [productFields])

  return (
    <ProductFields items={items} selected={selected} onSelect={setSelected}>
      {selected && <ProductFieldInfo node={selected} onSelect={setSelected} />}
    </ProductFields>
  )
})
