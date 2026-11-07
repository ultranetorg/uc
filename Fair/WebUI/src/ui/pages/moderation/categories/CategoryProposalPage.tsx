import { useTranslation } from "react-i18next"

import { useStoreContext } from "app"
import { useGetProposalDetails } from "entities"
import { useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { ProposalView } from "ui/views"
import { routes } from "utils"

import { CATEGORY_PROPOSALS_TAB_KEYS } from "./tabs"

export const CategoryProposalPage = () => {
  const { proposalId } = useParams()
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()
  const { t } = useTranslation()

  const { isLoading, data: proposal } = useGetProposalDetails(storeId, proposalId)

  useStoreTitle(store?.title, proposal?.title ? `Category - ${proposal?.title}` : "Category")

  return (
    <ProposalView
      parentBreadcrumbs={{ title: t("common:categories"), path: routes.moderation.categories(storeId!) }}
      isLoading={isLoading}
      proposal={proposal}
      previousPath={routes.moderation.categories(
        storeId!,
        proposal?.operation ? CATEGORY_PROPOSALS_TAB_KEYS[proposal.operation] : undefined,
      )}
    />
  )
}
