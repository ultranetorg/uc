import { memo } from "react"
import { useTranslation } from "react-i18next"

import { useSiteContext } from "app"
import { useGetModeratorDiscussion } from "entities"
import { useParams, useSiteTitle } from "hooks"
import { ProposalView } from "ui/views"
import { routes } from "utils"

export const PublisherProposalPage = memo(() => {
  const { siteId, proposalId } = useParams()
  const { site } = useSiteContext()
  const { t } = useTranslation()

  const { isFetching, data: proposal } = useGetModeratorDiscussion(siteId, proposalId)

  useSiteTitle(site?.title, proposal?.title ? `Publisher Proposal - ${proposal?.title}` : "Publisher Proposal")

  return (
    <ProposalView
      parentBreadcrumbs={[{ title: t("common:publishers"), path: routes.moderation.publishers(siteId!, "r") }]}
      isFetching={isFetching}
      proposal={proposal}
      previousPath={routes.moderation.publishers(siteId!, "r")}
    />
  )
})
