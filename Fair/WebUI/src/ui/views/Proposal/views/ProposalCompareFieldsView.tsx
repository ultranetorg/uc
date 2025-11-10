import { memo, useMemo } from "react"

import { PublicationUpdation } from "types"
import { ProductCompareFields } from "ui/components/proposal"

import { ProposalTypeViewProps } from "./types"

export const ProposalCompareFieldsView = memo(({ proposal }: ProposalTypeViewProps) => {
  const publicationIds = useMemo(
    () =>
      proposal?.options
        ?.map(option => option.operation)
        .filter((operation): operation is PublicationUpdation => operation.$type === "publication-updation")
        .map(operation => operation.publicationId),
    [proposal],
  )

  return <ProductCompareFields publicationIds={publicationIds} />
})
