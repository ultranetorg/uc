import { useTranslation } from "react-i18next"

import { useSiteContext } from "app"
import { useParams, useSiteTitle } from "hooks"
import { Breadcrumbs } from "ui/components"
import { AboutInfo } from "ui/components/specific"
import { routes } from "utils"

export const AboutPage = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("about")
  const { site } = useSiteContext()

  useSiteTitle(site?.title, "About")

  if (!site) {
    return <>LOADING 🕐</>
  }

  return (
    <div className="flex flex-col gap-6">
      <Breadcrumbs items={[{ path: routes.site(siteId!), title: t("common:home") }, { title: t("about") }]} />
      <AboutInfo className="max-w-160" title={site!.title} description={site!.description!} />
    </div>
  )
}
