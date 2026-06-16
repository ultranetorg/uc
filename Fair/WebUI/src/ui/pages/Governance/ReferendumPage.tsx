import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useGetAuthorReferendum } from "entities"
import { ProposalView } from "ui/views"

export const ReferendumPage = () => {
  const { siteId, referendumId } = useParams()
  const { t } = useTranslation()

  const { isFetching, data: proposal } = useGetAuthorReferendum(siteId, referendumId)

  return (
    <ProposalView
      parentBreadcrumbs={{ title: t("common:publisherReferendums"), path: `/${siteId}/g/r` }}
      isFetching={isFetching}
      proposal={proposal}
      previousPath={`/${siteId}/g/r/`}
    />
  )
}
