import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useGetModeratorDiscussion } from "entities"
import { ProposalView } from "ui/views"

export const ModeratorPublicationPage = () => {
  const { t } = useTranslation()
  const { siteId, discussionId } = useParams()

  const { isFetching, data: proposal } = useGetModeratorDiscussion(siteId, discussionId)

  return (
    <ProposalView
      parentBreadcrumb={{ title: t("common:moderation"), path: `/${siteId}/m/p/` }}
      isFetching={isFetching}
      proposal={proposal}
    />
  )
}
