import { useTranslation } from "react-i18next"

import { useStoreContext } from "app"
import { useResolveStoreId, useStoreTitle } from "hooks"
import { Breadcrumbs } from "ui/components"
import { AboutInfo } from "ui/components/specific"
import { routes } from "utils"

export const AboutPage = () => {
  const storeId = useResolveStoreId()
  const { t } = useTranslation("about")
  const { store: site } = useStoreContext()

  useStoreTitle(site?.title, "About")

  if (!site) {
    return <div>Loading</div>
  }

  return (
    <div className="flex flex-col gap-6">
      <Breadcrumbs items={[{ path: routes.store(storeId!), title: t("common:home") }, { title: t("about") }]} />
      <AboutInfo className="max-w-160" title={site!.title} description={site!.description!} />
    </div>
  )
}
