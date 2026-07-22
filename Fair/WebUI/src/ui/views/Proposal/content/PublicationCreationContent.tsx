import { memo, useMemo } from "react"
import { Proposal, PublicationCreation } from "types"

import { useGetProductDetails } from "entities"
import { ModerationPublicationHeader, ProductFieldsTree } from "ui/components/specific"

import { ProposalViewContentProps } from "./types"

const getProductId = (proposal: Proposal): string => (proposal.options[0].operation as PublicationCreation).productId

export const PublicationCreationContent = memo(({ siteId, proposal }: ProposalViewContentProps) => {
  const productId = useMemo(() => getProductId(proposal), [proposal])

  const { isFetching, data: product } = useGetProductDetails(productId)

  if (isFetching || !product) return <div>Loading</div>

  return (
    <div className="flex flex-col gap-6 rounded-lg bg-gray-100 p-6">
      <ModerationPublicationHeader
        storeId={siteId}
        title={product.title}
        logoId={product.logoId}
        authorId={product.authorId}
        authorTitle={product.authorTitle}
      />
      <ProductFieldsTree productFields={product.fields} />
    </div>
  )
})
