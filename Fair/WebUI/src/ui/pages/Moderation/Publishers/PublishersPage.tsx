import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"
import { ModerationHeader } from "ui/components/specific"

export const PublishersPage = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("publishersPage")

  return (
    <>
      <ModerationHeader title={t("title")} parentBreadcrumbs={{ path: `/${siteId}/m`, title: t("common:proposals") }} />
      PUBLISHERS PAGE
    </>
  )
}
