import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

export const useModeratorCategoryMenuItems = (categoryId: string, categoryTitle: string) => {
  const { t } = useTranslation("moderatorCategoryMenu")
  const { siteId } = useParams()

  const menuItems = useMemo(
    () => [
      {
        label: t("moderatorCategoryMenu:avatarChange"),
        to: `/${siteId}/m/new`,
        state: {
          title: `Change category "${categoryTitle}" avatar`,
          type: "category-avatar-change",
          categoryId,
          parentBreadcrumbs: [{ path: `/${siteId}/m/`, title: t("common:proposals") }],
          previousPath: `/${siteId}/m/`,
        },
      },
      {
        label: t("moderatorCategoryMenu:typeChange"),
        to: `/${siteId}/m/new`,
        state: {
          title: `Change category "${categoryTitle}" type`,
          type: "category-type-change",
          categoryId,
          parentBreadcrumbs: [{ path: `/${siteId}/m/`, title: t("common:proposals") }],
          previousPath: `/${siteId}/m/`,
        },
      },
      {
        label: t("moderatorCategoryMenu:move"),
        to: `/${siteId}/m/new`,
        state: {
          title: `Move "${categoryTitle}" category`,
          type: "category-movement",
          categoryId,
          parentBreadcrumbs: [
            { path: `/${siteId}/m/`, title: t("common:proposals") },
            { path: `/${siteId}/m/c/`, title: t("common:publications") },
          ],
          previousPath: `/${siteId}/m/c/`,
        },
      },
      {
        label: t("moderatorCategoryMenu:categoryCreate"),
        to: `/${siteId}/m/new`,
        state: {
          title: "Create new category",
          type: "category-creation",
          categoryId,
          parentBreadcrumbs: [
            { path: `/${siteId}/m/`, title: t("common:proposals") },
            { path: `/${siteId}/m/c/`, title: t("common:publications") },
          ],
          previousPath: `/${siteId}/m/c/`,
        },
      },
      {
        separator: true,
      },
      {
        label: t("moderatorCategoryMenu:remove"),
        to: `/${siteId}/m/new`,
        state: {
          title: `Remove category "${categoryTitle}"`,
          type: "category-deletion",
          categoryId,
          parentBreadcrumbs: [{ path: `/${siteId}/m/`, title: t("common:proposals") }],
          previousPath: `/${siteId}/m/`,
        },
      },
    ],
    [t, siteId, categoryTitle, categoryId],
  )

  return { menuItems }
}
