import { TFunction } from "i18next"
import { BreadcrumbsItemProps } from "ui/components"

import { CategoryPathItem } from "types"

import { routes } from "./routes"

export const createBreadcrumbs = (
  storeId: string,
  path: CategoryPathItem[] | undefined,
  title: string,
  t: TFunction,
): BreadcrumbsItemProps[] =>
  path
    ? [
        { path: routes.store(storeId), title: t("common:home") },
        ...path.map(x => ({ path: routes.category(storeId, x.id), title: x.title })),
        { title: title },
      ]
    : [{ path: routes.store(storeId), title: t("common:home") }, { title: title }]
