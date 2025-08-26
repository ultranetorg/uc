import { useParams } from "react-router-dom"

import { useGetAuthorReferendum } from "entities"
import { ProposalView } from "ui/views"
import { useTranslation } from "react-i18next"

export const ReferendumPage = () => {
  const { siteId, referendumId } = useParams()
  const { t } = useTranslation()

  const { isPending, data: referendum } = useGetAuthorReferendum(siteId, referendumId)

  if (isPending || !referendum) {
    return "Loading..."
  }

  return <ProposalView t={t} proposal={referendum} />
}
