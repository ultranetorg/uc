import { useMemo } from "react"
import { useTranslation } from "react-i18next"

export const useStoreDropdownMenuItems = (siteId: string) => {
  const { t } = useTranslation("storeDropdownMenu")

  const menuItems = useMemo(
    () => [
      { label: t("common:proposals"), to: `/${siteId}/m` },
      { label: t("common:moderators"), to: `/${siteId}/m/m` },
      { label: t("common:publications"), to: `/${siteId}/m/c` },
      { label: t("common:publishers"), to: `/${siteId}/m/a` },
      { label: t("common:reviews"), to: `/${siteId}/m/r` },
      { label: t("common:users"), to: `/${siteId}/m/u` },
      { separator: true },
      {
        label: t("common:settings"),
        children: [
          {
            label: t("avatarChange"),
            to: `/${siteId}/m/new`,
            state: {
              title: `Change site avatar`,
              type: "site-avatar-change",
              siteId,
              parentBreadcrumbs: [{ path: `/${siteId}/m/`, title: t("common:proposals") }],
              previousPath: `/${siteId}/m/`,
            },
          },
          {
            label: t("nameChange"),
            to: `/${siteId}/m/new`,
            state: {
              title: `Change site name`,
              type: "site-name-change",
              siteId,
              parentBreadcrumbs: [{ path: `/${siteId}/m/`, title: t("common:proposals") }],
              previousPath: `/${siteId}/m/`,
            },
          },
          {
            label: t("textChange"),
            to: `/${siteId}/m/new`,
            state: {
              title: `Change site text`,
              type: "site-text-change",
              siteId,
              parentBreadcrumbs: [{ path: `/${siteId}/m/`, title: t("common:proposals") }],
              previousPath: `/${siteId}/m/`,
            },
          },
        ],
      },
      { separator: true },
      { label: t("common:about"), to: `/${siteId}/i` },
    ],
    [siteId, t],
  )

  return { menuItems }
}
