import { memo, useMemo, useState } from "react"
import { ProductFieldViewModel } from "types"
import { ProductFieldInfo } from "./ProductFieldsTree/ProductFieldInfo.tsx"
import { useGetProductCompareFields } from "entities"
import { ProductFieldsView } from "./ProductFieldsView.tsx"
import { SelectedProps } from "./selected-props.ts"
import { mergeFields } from "./map-fields.ts"

export type ProductCompareFieldsProps = {
  publicationIds: string[]
}

interface ViewProps extends SelectedProps {
  publicationId: string
}

const View = memo(({ publicationId, selected, onSelect }: ViewProps) => {
  const response = useGetProductCompareFields(publicationId)
  const mergedResponse = useMemo(() => mergeFields(response), [response])

  return (
    <ProductFieldsView response={mergedResponse} selected={selected} onSelect={onSelect}>
      {selected && <ProductFieldInfo node={selected} onSelect={onSelect} />}
    </ProductFieldsView>
  )
})

export const ProductCompareFields = ({ publicationIds }: ProductCompareFieldsProps) => {
  const [selected, setSelected] = useState<ProductFieldViewModel | null>(null)

  return publicationIds.map(publicationId => (
    <View key={publicationId} publicationId={publicationId} selected={selected} onSelect={node => setSelected(node)} />
  ))
}
