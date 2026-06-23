import { useTranslation } from "react-i18next"

import { useSiteContext } from "app"
import { useGetModeratorDiscussion } from "entities"
import { useParams, useResolveSiteId, useSiteTitle } from "hooks"
import { ProposalView } from "ui/views"
import { routes } from "utils"

export const ModeratorPublicationPage = () => {
  const { proposalId } = useParams()
  const siteId = useResolveSiteId()
  const { site } = useSiteContext()
  const { t } = useTranslation()

  const { isFetching, data: proposal } = useGetModeratorDiscussion(siteId, proposalId)

  useSiteTitle(site?.title, proposal?.title ? `Publication - ${proposal?.title}` : "Publication")

  return (
    <ProposalView
      parentBreadcrumbs={{ title: t("common:publications"), path: routes.moderation.publications(siteId!) }}
      isFetching={isFetching}
      proposal={proposal}
      previousPath={routes.moderation.publications(siteId!, "p")}
    />
  )
}
