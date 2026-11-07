import { memo } from "react"
import { useTranslation } from "react-i18next"

import { useStoreContext } from "app"
import { useGetProposalDetails } from "entities"
import { useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { ProposalView } from "ui/views"
import { routes } from "utils"

export const PublisherProposalPage = memo(() => {
  const { proposalId } = useParams()
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()
  const { t } = useTranslation()

  const { isLoading, data: proposal } = useGetProposalDetails(storeId, proposalId)

  useStoreTitle(store?.title, proposal?.title ? `Publisher Proposal - ${proposal?.title}` : "Publisher Proposal")

  return (
    <ProposalView
      parentBreadcrumbs={[{ title: t("common:publishers"), path: routes.moderation.publishers(storeId!, "proposals") }]}
      isLoading={isLoading}
      proposal={proposal}
      previousPath={routes.moderation.publishers(storeId!, "proposals")}
    />
  )
})
