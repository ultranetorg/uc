import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useSiteContext } from "app"
import { useGetModeratorDiscussion } from "entities"
import { useSiteTitle } from "hooks"
import { ProposalView } from "ui/views"
import { routes } from "utils"

export const ProposalPage = () => {
  const { siteId, discussionId } = useParams()
  const { site } = useSiteContext()
  const { t } = useTranslation()

  const { isFetching, data: proposal } = useGetModeratorDiscussion(siteId, discussionId)

  useSiteTitle(site?.title, proposal?.title ? `Proposal - ${proposal?.title}` : "Proposal")

  return (
    <ProposalView
      isFetching={isFetching}
      proposal={proposal}
      parentBreadcrumbs={{ title: t("common:moderatorProposals"), path: routes.moderation.root(siteId!) }}
    />
  )
}
