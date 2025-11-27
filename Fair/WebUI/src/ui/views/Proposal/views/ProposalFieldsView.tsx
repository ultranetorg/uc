import { memo, useMemo } from "react"

import { ProductFields } from "ui/components/proposal"

import { PublicationCreation } from "types"
import { ProposalTypeViewProps } from "./types"

export const ProposalFieldsView = memo(({ proposal }: ProposalTypeViewProps) => {
  const productIds = useMemo(
    () =>
      proposal?.options
        ?.map(option => option.operation)
        .filter((operation): operation is PublicationCreation => operation.$type === "publication-creation")
        .map(operation => operation.productId),
    [proposal],
  )

  return <ProductFields productIds={productIds} />
})
