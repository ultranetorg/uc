import { useMemo, useState } from "react"
import { ProductFieldViewModel } from "types"
import { useGetProductFields } from "entities"
import { ProductFieldsView } from "./ProductFieldsView.tsx"
import { ProductFieldInfo } from "./ProductFieldsTree/ProductFieldInfo.tsx"
import { SelectedProps } from "./selected-props.ts"
import { mapFields } from "./map-fields.ts"

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
