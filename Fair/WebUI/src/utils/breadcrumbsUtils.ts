import { TFunction } from "i18next"
import { BreadcrumbsItemProps } from "ui/components"

import { routes } from "./routes"

export const createBreadcrumbs = (
  siteId: string,
  parentId: string,
  parentTitle: string,
  title: string,
  t: TFunction,
): BreadcrumbsItemProps[] =>
  parentId
    ? [
        { path: routes.site(siteId), title: t("common:home") },
        { path: routes.category(siteId, parentId), title: parentTitle },
        { title: title },
      ]
    : [{ path: routes.site(siteId), title: t("common:home") }, { title: title }]
