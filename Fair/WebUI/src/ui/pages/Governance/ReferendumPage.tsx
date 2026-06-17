import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useSiteContext } from "app"
import { useGetAuthorReferendum } from "entities"
import { useSiteTitle } from "hooks"
import { ProposalView } from "ui/views"

export const ReferendumPage = () => {
  const { siteId, referendumId } = useParams()
  const { site } = useSiteContext()
  const { t } = useTranslation()

  const { isFetching, data: proposal } = useGetAuthorReferendum(siteId, referendumId)

  useSiteTitle(site?.title, proposal?.title ? `Referendum - ${proposal?.title}` : "Referendum")

  return (
    <ProposalView
      parentBreadcrumbs={{ title: t("common:publisherReferendums"), path: `/${siteId}/g/r` }}
      isFetching={isFetching}
      proposal={proposal}
      previousPath={`/${siteId}/g/r/`}
    />
  )
}
