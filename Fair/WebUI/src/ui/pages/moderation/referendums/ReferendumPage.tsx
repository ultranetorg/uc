import { useTranslation } from "react-i18next"

import { useStoreContext } from "app"
import { useGetProposalDetails } from "entities"
import { useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { ProposalView } from "ui/views"
import { routes } from "utils"

export const ReferendumPage = () => {
  const { referendumId } = useParams()
  const { store } = useStoreContext()
  const storeId = useResolveStoreId()
  const { t } = useTranslation()

  const { isFetching, data: proposal } = useGetProposalDetails(storeId, referendumId)

  useStoreTitle(store?.title, proposal?.title ? `Referendum - ${proposal?.title}` : "Referendum")

  return (
    <ProposalView
      parentBreadcrumbs={{ title: t("common:referendums"), path: routes.moderation.referendums(storeId!) }}
      isFetching={isFetching}
      proposal={proposal}
      previousPath={routes.moderation.referendums(storeId!)}
    />
  )
}
