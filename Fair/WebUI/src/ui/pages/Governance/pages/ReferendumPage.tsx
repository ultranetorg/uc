import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useGetAuthorReferendum } from "entities"
import { ProposalView } from "ui/views"

export const ReferendumPage = () => {
  const { t } = useTranslation()
  const { siteId, referendumId } = useParams()

  const { isFetching, data: proposal } = useGetAuthorReferendum(siteId, referendumId)

  return (
    <ProposalView
      parentBreadcrumb={{ title: t("common:governance"), path: `/${siteId}/g` }}
      isFetching={isFetching}
      proposal={proposal}
    />
  )
}
