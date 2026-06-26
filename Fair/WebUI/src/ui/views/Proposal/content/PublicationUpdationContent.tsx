import { memo, useMemo } from "react"

import { useGetPublicationDetailsDiff } from "entities"
import { Proposal, PublicationUpdation } from "types"
import { ModerationPublicationHeader, ProductFieldsDiff } from "ui/components/specific"

import { ProposalViewContentProps } from "./types"

const getOperationData = (proposal: Proposal): { publicationId: string; version: number } =>
  proposal.options[0].operation as PublicationUpdation

export const PublicationUpdationContent = memo(({ siteId, proposal }: ProposalViewContentProps) => {
  const data = useMemo(() => getOperationData(proposal), [proposal])

  const { isFetching, data: publication } = useGetPublicationDetailsDiff(data.publicationId, data.version)

  if (isFetching || !publication) return <div>Loading</div>

  return (
    <div className="flex flex-col gap-6 rounded-lg bg-gray-100 p-6">
      <ModerationPublicationHeader
        siteId={siteId}
        title={publication.title}
        logoId={publication.logoId}
        authorId={publication.authorId}
        authorTitle={publication.authorTitle}
      />
      <ProductFieldsDiff from={publication.fields} to={publication.fieldsTo} />
    </div>
  )
})
