import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { useSiteContext } from "app"
import { Breadcrumbs } from "ui/components"
import { AboutInfo } from "ui/components/specific"

export const AboutPage = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("about")
  const { site } = useSiteContext()

  return (
    <div className="flex flex-col gap-6">
      <Breadcrumbs items={[{ path: `/${siteId}`, title: t("common:home") }, { title: t("about") }]} />
      <AboutInfo className="max-w-160" title={site?.title}>
        {site?.description}
      </AboutInfo>
    </div>
  )
}
