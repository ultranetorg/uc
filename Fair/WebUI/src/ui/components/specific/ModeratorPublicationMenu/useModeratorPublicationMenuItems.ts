import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

export const useModeratorPublicationMenuItems = (publicationId: string, publicationTitle?: string) => {
  const { siteId } = useParams()
  const { t } = useTranslation("moderatorPublicationMenu")

  const menuItems = useMemo(
    () => [
      {
        label: t("unpublishPublication"),
        to: `/${siteId}/m/new`,
        state: {
          title: publicationTitle ? `Unpublish publication "${publicationTitle}"` : "Unpublish publication",
          type: "publication-unpublish",
          publicationId,
          parentBreadcrumbs: [
            { path: `/${siteId}/m/`, title: t("common:proposals") },
            { path: `/${siteId}/m/c/`, title: t("common:publications") },
          ],
          previousPath: `/${siteId}/m/c/`,
        },
      },
      { separator: true },
      {
        label: t("removePublication"),
        to: `/${siteId}/m/new`,
        state: {
          title: publicationTitle ? `Remove publication "${publicationTitle}"` : "Remove publication",
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
