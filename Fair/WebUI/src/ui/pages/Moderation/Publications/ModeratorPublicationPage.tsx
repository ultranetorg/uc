import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useSiteContext } from "app"
import { useGetModeratorDiscussion } from "entities"
import { useSiteTitle } from "hooks"
import { ProposalView } from "ui/views"

export const ModeratorPublicationPage = () => {
  const { siteId, proposalId } = useParams()
  const { site } = useSiteContext()
  const { t } = useTranslation()

  const { isFetching, data: proposal } = useGetModeratorDiscussion(siteId, proposalId)

  useSiteTitle(site?.title, proposal?.title ? `Publication - ${proposal?.title}` : "Publication")

  return (
    <ProposalView
      parentBreadcrumbs={{ title: t("common:publications"), path: `/${siteId}/m/c` }}
      isFetching={isFetching}
      proposal={proposal}
      previousPath={`/${siteId}/m/c/p`}
    />
  )
}
