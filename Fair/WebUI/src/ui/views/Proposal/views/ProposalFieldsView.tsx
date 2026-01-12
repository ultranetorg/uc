import { memo, useMemo } from "react"

import { useGetProductFields } from "entities"
import { ProductFieldsTree } from "ui/components/specific"

import { PublicationCreation } from "types"
import { ProposalTypeViewProps } from "./types"

const ProductFieldsForProduct = ({ productId }: { productId: string }) => {
  const { isFetching, data } = useGetProductFields(productId)

  if (isFetching || !data) return <div>LOADING</div>

  return <ProductFieldsTree productFields={data} />
}

export const ProposalFieldsView = memo(({ proposal }: ProposalTypeViewProps) => {
  const productIds = useMemo(
    () =>
      proposal?.options
        ?.map(option => option.operation)
        .filter((operation): operation is PublicationCreation => operation.$type === "publication-creation")
        .map(operation => operation.productId),
    [proposal],
  )

  if (!productIds?.length) return null

  return (
    <>
      {productIds.map(productId => (
        <ProductFieldsForProduct key={productId} productId={productId} />
      ))}
    </>
  )
})
