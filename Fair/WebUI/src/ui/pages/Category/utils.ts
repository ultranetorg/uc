import { TFunction } from "i18next"
import { BreadcrumbsItemProps } from "ui/components"

export const createBreadcrumbs = (
  siteId: string,
  parentId: string,
  parentTitle: string,
  title: string,
  t: TFunction,
): BreadcrumbsItemProps[] =>
  parentId
    ? [
        { path: `/${siteId}`, title: t("home") },
        { path: `/${siteId}/c/${parentId}`, title: parentTitle },
        { title: title },
      ]
    : [{ path: `/${siteId}`, title: t("home") }, { title: title }]
