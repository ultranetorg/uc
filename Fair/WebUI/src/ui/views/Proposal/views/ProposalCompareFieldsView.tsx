import { memo } from "react"

import { useGetProductCompareFields } from "entities"
import { ProductFieldsDiff } from "ui/components/specific"
import { PublicationUpdation } from "types"
import { getFirstOperation } from "utils"

import { ProposalTypeViewProps } from "./types"

export const ProposalCompareFieldsView = memo(({ proposal }: ProposalTypeViewProps) => {
  const operation = getFirstOperation<PublicationUpdation>(proposal, "publication-updation")
  const { data: fields } = useGetProductCompareFields(operation?.publicationId, operation?.version)

  return fields !== undefined ? <ProductFieldsDiff from={fields.from} to={fields.to} /> : null
})
