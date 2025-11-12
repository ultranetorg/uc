import { memo, useMemo, useState } from "react"
import { ProductFieldViewModel } from "types"
import { ProductFieldInfo } from "./ProductFieldsTree/ProductFieldInfo"
import { useGetProductCompareFields } from "entities"
import { ProductFieldsView } from "./ProductFieldsView"
import { mergeFields } from "./utils"
import { SelectedProps } from "./types"

export type ProductCompareFieldsProps = {
  publications: { id: string, version: number }[]
}

interface ViewProps extends SelectedProps {
  publicationId: string,
  version: number
}

const View = memo(({ publicationId, version, selected, onSelect }: ViewProps) => {
  const response = useGetProductCompareFields(publicationId, version)
  const mergedResponse = useMemo(() => mergeFields(response), [response])

  return (
    <ProductFieldsView response={mergedResponse} selected={selected} onSelect={onSelect}>
      {selected && <ProductFieldInfo node={selected} onSelect={onSelect} />}
    </ProductFieldsView>
  )
})

export const ProductCompareFields = ({ publications }: ProductCompareFieldsProps) => {
  const [selected, setSelected] = useState<ProductFieldViewModel | null>(null)

  return publications.map(publication => (
    <View key={publication.id} publicationId={publication.id} version={publication.version} selected={selected} onSelect={node => setSelected(node)} />
  ))
}
