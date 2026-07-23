import { useTranslation } from "react-i18next"

import { useStoreContext } from "app"
import { useGetModeratorDiscussion } from "entities"
import { useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { ProposalView } from "ui/views"
import { routes } from "utils"

export const ModeratorPublicationPage = () => {
  const { proposalId } = useParams()
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()
  const { t } = useTranslation()

  const { isFetching, data: proposal } = useGetModeratorDiscussion(storeId, proposalId)

  useStoreTitle(store?.title, proposal?.title ? `Publication - ${proposal?.title}` : "Publication")

  return (
    <ProposalView
      parentBreadcrumbs={{ title: t("common:publications"), path: routes.moderation.publications(storeId!) }}
      isFetching={isFetching}
      proposal={proposal}
      previousPath={routes.moderation.publications(storeId!, "proposals")}
    />
  )
}
