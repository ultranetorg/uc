import { TFunction } from "i18next"
import { BreadcrumbsItemProps } from "ui/components"

import { CategoryPathItem } from "types"

import { routes } from "./routes"

export const createBreadcrumbs = (
  siteId: string,
  path: CategoryPathItem[] | undefined,
  title: string,
  t: TFunction,
): BreadcrumbsItemProps[] =>
  path
    ? [
        { path: routes.site(siteId), title: t("common:home") },
        ...path.map(x => ({ path: routes.category(siteId, x.id), title: x.title })),
        { title: title },
      ]
    : [{ path: routes.site(siteId), title: t("common:home") }, { title: title }]
