import { memo, useMemo, useState } from "react"

import { ProductField } from "types"

import { ProductFields } from "./ProductFields"
import { ProductFieldInfo } from "./ProductFieldInfo"
import { ProductFieldViewModel } from "./types"
import { mergeFields } from "./utils"

export type ProductFieldsDiffProps = {
  from?: ProductField[]
  to?: ProductField[]
}

export const ProductFieldsDiff = memo(({ from, to }: ProductFieldsDiffProps) => {
  const [selected, setSelected] = useState<ProductFieldViewModel | undefined>()

  const items = useMemo(() => mergeFields({ from: from ?? [], to: to ?? [] }), [from, to])

  return (
    <ProductFields items={items} selected={selected} onSelect={setSelected}>
      {selected && <ProductFieldInfo node={selected} onSelect={setSelected} />}
    </ProductFields>
  )
})
