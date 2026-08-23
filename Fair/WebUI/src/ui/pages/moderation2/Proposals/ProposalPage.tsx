import { useTranslation } from "react-i18next"

import { useStoreContext } from "app"
import { useGetModeratorDiscussion } from "entities"
import { useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { ProposalView } from "ui/views"
import { routes } from "utils"

export const ProposalPage = () => {
  const { proposalId } = useParams()
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()
  const { t } = useTranslation()

  const { isFetching, data: proposal } = useGetModeratorDiscussion(storeId, proposalId)

  useStoreTitle(store?.title, proposal?.title ? `Proposal - ${proposal?.title}` : "Proposal")

  return (
    <ProposalView
      isFetching={isFetching}
      proposal={proposal}
      parentBreadcrumbs={{ title: t("common:moderatorProposals"), path: routes.moderation.proposals(storeId!) }}
    />
  )
}
