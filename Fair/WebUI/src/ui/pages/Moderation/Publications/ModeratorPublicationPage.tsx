import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useGetModeratorDiscussion } from "entities"
import { ProposalView } from "ui/views"

export const ModeratorPublicationPage = () => {
  const { t } = useTranslation()
  const { siteId, publicationId } = useParams()

  const { isFetching, data: proposal } = useGetModeratorDiscussion(siteId, publicationId)

  return (
    <ProposalView
      parentBreadcrumb={{ title: t("common:publications"), path: `/${siteId}/m/c` }}
      isFetching={isFetching}
      proposal={proposal}
    />
  )
}
