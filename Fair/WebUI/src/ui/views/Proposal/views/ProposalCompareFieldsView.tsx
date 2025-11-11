import { memo, useMemo } from "react"

import { PublicationUpdation } from "types"
import { ProductCompareFields } from "ui/components/proposal"

import { ProposalTypeViewProps } from "./types"

export const ProposalCompareFieldsView = memo(({ proposal }: ProposalTypeViewProps) => {
  const publications = useMemo(
    () =>
      proposal?.options
        ?.map(option => option.operation)
        .filter((operation): operation is PublicationUpdation => operation.$type === "publication-updation")
        .map(operation => ({
          id: operation.publicationId,
          version: operation.version,
        })),
    [proposal],
  )

  return <ProductCompareFields publications={publications} />
})
