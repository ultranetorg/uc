import { memo } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { useSiteContext } from "app"
import { useGetModeratorDiscussion } from "entities"
import { useSiteTitle } from "hooks"
import { ProposalView } from "ui/views"

export const PublisherProposalPage = memo(() => {
  const { siteId, proposalId } = useParams()
  const { site } = useSiteContext()
  const { t } = useTranslation()

  const { isFetching, data: proposal } = useGetModeratorDiscussion(siteId, proposalId)

  useSiteTitle(site?.title, proposal?.title ? `Publisher Proposal - ${proposal?.title}` : "Publisher Proposal")

  return (
    <ProposalView
      parentBreadcrumbs={[{ title: t("common:publishers"), path: `/${siteId}/m/a/r` }]}
      isFetching={isFetching}
      proposal={proposal}
      previousPath={`/${siteId}/m/a/r`}
    />
  )
})
