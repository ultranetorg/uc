import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"
import { truncate } from "lodash"

import { unpublishedPublicationsKeys } from "entities"

export const useModeratorPublicationMenuItems = (publicationId: string, publicationTitle?: string) => {
  const { siteId } = useParams()
  const { t } = useTranslation("moderatorPublicationMenu")

  const menuItems = useMemo(
    () => [
      {
        label: t("unpublishPublication"),
        to: `/${siteId}/m/new`,
        state: {
          title: publicationTitle
            ? `Unpublish publication "${truncate(publicationTitle, { length: 40 })}"`
            : "Unpublish publication",
          type: "publication-unpublish",
          publicationId,
          parentBreadcrumbs: [
            { path: `/${siteId}/m/`, title: t("common:proposals") },
            { path: `/${siteId}/m/c/`, title: t("common:publications") },
          ],
          previousPath: `/${siteId}/m/c/u`,
          invalidateQueryKeys: unpublishedPublicationsKeys.all(siteId!),
        },
      },
      { separator: true },
      {
        label: t("removePublication"),
        to: `/${siteId}/m/new`,
        state: {
          title: publicationTitle
            ? `Remove publication "${truncate(publicationTitle, { length: 43 })}"`
            : "Remove publication",
          type: "publication-deletion",
          publicationId,
          parentBreadcrumbs: [
            { path: `/${siteId}/m/`, title: t("common:proposals") },
            { path: `/${siteId}/m/c/`, title: t("common:publications") },
          ],
          previousPath: `/${siteId}/m/c/`,
        },
      },
    ],
    [publicationId, publicationTitle, siteId, t],
  )

  return { menuItems }
}
