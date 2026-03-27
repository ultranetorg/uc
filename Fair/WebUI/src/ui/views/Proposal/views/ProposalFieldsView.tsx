import { memo, useMemo } from "react"

import { useGetProductFields } from "entities"
import { ProposalDetails, PublicationCreation, PublicationDeletion } from "types"
import { ProductFieldsTree } from "ui/components/specific"

import { ProposalTypeViewProps } from "./types"

const getPublicationOrProductId = (proposal?: ProposalDetails): string | undefined => {
  const operation = proposal?.options.find(
    x => x.operation.$type === "publication-creation" || x.operation.$type === "publication-deletion",
  )?.operation
  if (operation?.$type === "publication-creation") return (operation as PublicationCreation | undefined)?.productId
  else if (operation?.$type === "publication-deletion")
    return (operation as PublicationDeletion | undefined)?.publicationId
  return undefined
}

export const PublicationCreationProposalView = memo(({ proposal }: ProposalTypeViewProps) => {
  const productId = useMemo(() => getPublicationOrProductId(proposal), [proposal])

  const { isFetching, data: productFields } = useGetProductFields(productId)

  if (isFetching || !productFields) return <div>LOADING</div>

  return <ProductFieldsTree productFields={productFields} />
})
