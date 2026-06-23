import { useTranslation } from "react-i18next"

import { useSiteContext } from "app"
import { useGetAuthorReferendum } from "entities"
import { useParams, useSiteTitle } from "hooks"
import { ProposalView } from "ui/views"
import { routes } from "utils"

export const ReferendumPage = () => {
  const { siteId, referendumId } = useParams()
  const { site } = useSiteContext()
  const { t } = useTranslation()

  const { isFetching, data: proposal } = useGetAuthorReferendum(siteId, referendumId)

  useSiteTitle(site?.title, proposal?.title ? `Referendum - ${proposal?.title}` : "Referendum")

  return (
    <ProposalView
      parentBreadcrumbs={{ title: t("common:publisherReferendums"), path: routes.governance.referendums(siteId!) }}
      isFetching={isFetching}
      proposal={proposal}
      previousPath={routes.governance.referendums(siteId!)}
    />
  )
}
