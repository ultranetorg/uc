import { useTranslation } from "react-i18next"

import { useStoreContext } from "app"
import { useGetAuthorReferendum } from "entities"
import { useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { ProposalView } from "ui/views"
import { routes } from "utils"

export const ReferendumPage = () => {
  const { referendumId } = useParams()
  const { store } = useStoreContext()
  const storeId = useResolveStoreId()
  const { t } = useTranslation()

  const { isFetching, data: proposal } = useGetAuthorReferendum(storeId, referendumId)

  useStoreTitle(store?.title, proposal?.title ? `Referendum - ${proposal?.title}` : "Referendum")

  return (
    <ProposalView
      parentBreadcrumbs={{ title: t("common:publisherReferendums"), path: routes.governance.referendums(storeId!) }}
      isFetching={isFetching}
      proposal={proposal}
      previousPath={routes.governance.referendums(storeId!)}
    />
  )
}
