import { memo, useMemo } from "react"

import { useGetPublicationDetails } from "entities"
import { Proposal, PublicationDeletion } from "types"
import { ModerationPublicationHeader, ProductFieldsTree } from "ui/components/specific"

import { ProposalViewContentProps } from "./types"

const getPublicationId = (proposal: Proposal): string =>
  (proposal.options[0].operation as PublicationDeletion).publicationId

export const PublicationDeletionContent = memo(({ proposal }: ProposalViewContentProps) => {
  const publicationId = useMemo(() => getPublicationId(proposal), [proposal])

  const { isPending, data: publication } = useGetPublicationDetails(publicationId)

  if (isPending || !publication) return <div>LOADING</div>

  return (
    <div className="flex flex-col gap-6">
      <ModerationPublicationHeader
        title={publication.title}
        logoId={publication.logoId}
        authorId={publication.authorId}
        authorTitle={publication.authorTitle}
      />
      <ProductFieldsTree productFields={publication.fields} />
    </div>
  )
})
