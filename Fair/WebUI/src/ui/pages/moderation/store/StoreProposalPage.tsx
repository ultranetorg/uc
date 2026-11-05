import { useTranslation } from "react-i18next"

import { useStoreContext } from "app"
import { useGetProposalDetails } from "entities"
import { useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { ProposalView } from "ui/views"
import { routes } from "utils"

import { STORE_PROPOSALS_TAB_KEYS } from "./tabs"

export const StoreProposalPage = () => {
  const { proposalId } = useParams()
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()
  const { t } = useTranslation()

  const { isLoading, data: proposal } = useGetProposalDetails(storeId, proposalId)

  useStoreTitle(store?.title, proposal?.title ? `Store - ${proposal?.title}` : "Store")

  return (
    <ProposalView
      parentBreadcrumbs={{ title: t("common:store"), path: routes.moderation.store(storeId!) }}
      isLoading={isLoading}
      proposal={proposal}
      previousPath={routes.moderation.store(
        storeId!,
        proposal?.operation ? STORE_PROPOSALS_TAB_KEYS[proposal.operation] : undefined,
      )}
    />
  )
}
