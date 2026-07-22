import { memo } from "react"
import { useTranslation } from "react-i18next"

import { useStoreContext } from "app"
import { useGetAuthorReferendum } from "entities"
import { useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { ProposalView } from "ui/views"
import { routes } from "utils"

export const ModeratorProposalPage = memo(() => {
  const { t } = useTranslation()
  const { proposalId } = useParams()
  const storeId = useResolveStoreId()
  const { store: site } = useStoreContext()

  const { isFetching, data: proposal } = useGetAuthorReferendum(storeId, proposalId)

  useStoreTitle(site?.title, proposal?.title ? `Moderator Proposal - ${proposal?.title}` : "Moderator Proposal")

  return (
    <ProposalView
      parentBreadcrumbs={[{ title: t("common:moderators"), path: routes.moderation.moderators(storeId!, "proposals") }]}
      isFetching={isFetching}
      proposal={proposal}
      previousPath={routes.moderation.moderators(storeId!, "proposals")}
    />
  )
})
