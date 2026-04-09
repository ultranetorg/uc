import { memo, useMemo } from "react"
import { Proposal, PublicationCreation } from "types"

import { ModerationPublicationHeader, ProductFieldsTree } from "ui/components/specific"
import { useGetProductDetails } from "entities/Product"

import { ProposalViewContentProps } from "./types"

const getProductId = (proposal: Proposal): string => (proposal.options[0].operation as PublicationCreation).productId

export const PublicationCreationContent = memo(({ proposal }: ProposalViewContentProps) => {
  const productId = useMemo(() => getProductId(proposal), [proposal])

  const { isFetching, data: product } = useGetProductDetails(productId)

  if (isFetching || !product) return <div>LOADING</div>

  return (
    <div className="flex flex-col gap-6">
      <ModerationPublicationHeader
        title={product.title}
        logoId={product.logoId}
        authorId={product.authorId}
        authorTitle={product.authorTitle}
      />
      <ProductFieldsTree productFields={product.fields} />
    </div>
  )
})
