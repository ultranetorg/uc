import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useGetModeratorDiscussion } from "entities"
import { ProposalView } from "ui/views"

export const ProposalPage = () => {
  const { siteId, discussionId } = useParams()
  const { t } = useTranslation()

  const { isFetching, data: proposal } = useGetModeratorDiscussion(siteId, discussionId)

  return (
    <ProposalView
      isFetching={isFetching}
      proposal={proposal}
      parentBreadcrumbs={{ title: t("common:moderatorProposals"), path: `/${siteId}/m` }}
    />
  )
}
