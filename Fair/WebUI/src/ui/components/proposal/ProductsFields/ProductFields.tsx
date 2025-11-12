import { useMemo, useState } from "react"
import { ProductFieldViewModel } from "types"
import { useGetProductFields } from "entities"
import { ProductFieldsView } from "./ProductFieldsView"
import { ProductFieldInfo } from "./ProductFieldsTree/ProductFieldInfo"
import { mapFields } from "./utils"
import { SelectedProps } from "./types"

export interface ProductsFieldsTreeProps {
  productIds: string[]
}

interface ViewProps extends SelectedProps {
  productId: string
}

const View = ({ productId, selected, onSelect }: ViewProps) => {
  const response = useGetProductFields(productId)
  const mappedResponse = useMemo(() => mapFields(response), [response])

  return (
    <ProductFieldsView response={mappedResponse} selected={selected} onSelect={onSelect}>
      {selected && <ProductFieldInfo node={selected} onSelect={onSelect} />}
    </ProductFieldsView>
  )
}

export const ProductFields = ({ productIds }: ProductsFieldsTreeProps) => {
  const [selected, setSelected] = useState<ProductFieldViewModel | null>(null)

  return productIds.map(productId => (
    <View key={productId} productId={productId} selected={selected} onSelect={node => setSelected(node)} />
  ))
}
