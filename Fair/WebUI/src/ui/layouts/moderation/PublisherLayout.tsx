import { memo, PropsWithChildren } from "react"
import { Outlet, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useResolveSiteId } from "hooks"
import { Breadcrumbs } from "ui/components"
import { routes } from "utils"

export const PublishersLayout = memo(({ children }: PropsWithChildren) => {
  const siteId = useResolveSiteId()
  const { publisherId } = useParams()
  const { t } = useTranslation("usersPage")

  return (
    <>
      <Breadcrumbs
        fullPath={true}
        items={[
          { path: routes.site(siteId!), title: t("common:home") },
          { path: routes.moderation.publishers(siteId!), title: t("common:publishers") },
          { title: publisherId! },
        ]}
      />
      {children ?? <Outlet />}
    </>
  )
})
