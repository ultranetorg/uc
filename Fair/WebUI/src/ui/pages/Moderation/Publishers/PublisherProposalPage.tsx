import { memo } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { useGetModeratorDiscussion } from "entities"
import { ProposalView } from "ui/views"

export const PublisherProposalPage = memo(() => {
  const { t } = useTranslation()
  const { siteId, proposalId } = useParams()

  const { isFetching, data: proposal } = useGetModeratorDiscussion(siteId, proposalId)

  return (
    <ProposalView
      parentBreadcrumbs={[{ title: t("common:publishers"), path: `/${siteId}/m/a/r` }]}
      isFetching={isFetching}
      proposal={proposal}
      previousPath={`/${siteId}/m/m/p`}
    />
  )
})
