import { memo } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { useGetAuthorReferendum } from "entities"
import { ProposalView } from "ui/views"

export const ModeratorProposalPage = memo(() => {
  const { t } = useTranslation()
  const { siteId, proposalId } = useParams()

  const { isFetching, data: proposal } = useGetAuthorReferendum(siteId, proposalId)

  return (
    <ProposalView
      parentBreadcrumbs={[{ title: t("common:moderators"), path: `/${siteId}/m/m/p` }]}
      isFetching={isFetching}
      proposal={proposal}
      previousPath={`/${siteId}/m/m/p`}
    />
  )
})
