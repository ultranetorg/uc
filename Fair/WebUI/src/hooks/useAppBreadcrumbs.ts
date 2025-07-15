import { useMemo } from "react"
import { matchRoutes, useLocation } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { routes } from "app"
import { BreadcrumbsItem2 } from "ui/components"

export const useAppBreadcrumbs = (): BreadcrumbsItem2[] => {
  const location = useLocation()
  const { t } = useTranslation()

  const breadcrumbs = useMemo(() => {
    const matchedRoutes = matchRoutes(routes, location.pathname)
    return matchedRoutes && matchedRoutes.length > 1
      ? matchedRoutes.slice(1).map(({ route, pathname, params }) => {
          const label = typeof route.breadcrumb === "function" ? route.breadcrumb(t, params) : t(route.breadcrumb || "")

          return {
            path: pathname,
            title: label,
          }
        })
      : []
  }, [location.pathname, t])

  return breadcrumbs
}
