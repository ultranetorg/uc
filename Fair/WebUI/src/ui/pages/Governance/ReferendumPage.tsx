import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useGetAuthorReferendum, useGetAuthorReferendumComment } from "entities"
import { ProposalView } from "ui/views"

export const ReferendumPage = () => {
  const { t } = useTranslation()
  const { siteId, referendumId } = useParams()

  const { isFetching, data: proposal } = useGetAuthorReferendum(siteId, referendumId)
  const { isFetching: isCommentsFetching, data: comments } = useGetAuthorReferendumComment(siteId, proposal?.id)

  return (
    <ProposalView
      parentBreadcrumb={{ title: t("common:governance"), path: `/${siteId}/g` }}
      isFetching={isFetching}
      proposal={proposal}
      isCommentsFetching={isCommentsFetching}
      comments={comments}
    />
  )
}
