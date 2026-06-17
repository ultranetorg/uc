import { memo } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { useSiteContext } from "app"
import { useGetAuthorReferendum } from "entities"
import { useSiteTitle } from "hooks"
import { ProposalView } from "ui/views"
import { routes } from "utils"

export const ModeratorProposalPage = memo(() => {
  const { t } = useTranslation()
  const { siteId, proposalId } = useParams()
  const { site } = useSiteContext()

  const { isFetching, data: proposal } = useGetAuthorReferendum(siteId, proposalId)

  useSiteTitle(site?.title, proposal?.title ? `Moderator Proposal - ${proposal?.title}` : "Moderator Proposal")

  return (
    <ProposalView
      parentBreadcrumbs={[{ title: t("common:moderators"), path: routes.moderation.moderators(siteId!) }]}
      isFetching={isFetching}
      proposal={proposal}
      previousPath={routes.moderation.moderators(siteId!, "p")}
    />
  )
})
