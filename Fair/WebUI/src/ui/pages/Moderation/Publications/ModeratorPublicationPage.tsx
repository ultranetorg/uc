import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useGetModeratorDiscussion } from "entities"
import { ProposalView } from "ui/views"

export const ModeratorPublicationPage = () => {
  const { t } = useTranslation()
  const { siteId, proposalId } = useParams()

  const { isFetching, data: proposal } = useGetModeratorDiscussion(siteId, proposalId)

  return (
    <ProposalView
      parentBreadcrumbs={{ title: t("common:publications"), path: `/${siteId}/m/c` }}
      isFetching={isFetching}
      proposal={proposal}
      previousPath={`/${siteId}/m/c/p`}
    />
  )
}
