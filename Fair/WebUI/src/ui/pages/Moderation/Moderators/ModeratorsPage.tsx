import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { useGetSiteModerators } from "entities"
import { ModerationHeader } from "ui/components/specific"

export const ModeratorsPage = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("moderatorsPage")

  const { data: moderators } = useGetSiteModerators(siteId)

  return (
    <>
      <ModerationHeader title={t("title")} parentBreadcrumbs={{ path: `/${siteId}/m`, title: t("common:proposals") }} />
      {JSON.stringify(moderators)}
    </>
  )
}
