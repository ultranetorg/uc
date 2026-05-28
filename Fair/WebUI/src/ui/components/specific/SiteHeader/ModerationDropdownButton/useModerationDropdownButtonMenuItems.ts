import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useLocation } from "react-router-dom"

import { useSiteRolesContext } from "app"
import { sitesKeys } from "entities"
import { isModeratorVoting } from "utils"
import { SimpleMenuItem } from "ui/components/SimpleMenu"

export const useModerationDropdownButtonMenuItems = (siteId: string): SimpleMenuItem[] => {
  const location = useLocation()
  const { policies } = useSiteRolesContext()
  const { t } = useTranslation("storeDropdownMenu")

  const siteItems = useMemo(
    () => [
      ...(isModeratorVoting("site-avatar-change", policies)
        ? [
            {
              label: t("avatarChange"),
              to: `/${siteId}/m/new`,
              state: {
                title: `Change site avatar`,
                type: "site-avatar-change",
                siteId,
                parentBreadcrumbs: [{ path: `/${siteId}/m/`, title: t("common:proposals") }],
                redirectAfterProposalCreation: `/${siteId}/m/`,
                redirectAfterProposalExecution: location.pathname,
                invalidateQueryKeys: sitesKeys.detail(siteId),
              },
            },
          ]
        : []),
      ...(isModeratorVoting("site-name-change", policies)
        ? [
            {
              label: t("nameChange"),
              to: `/${siteId}/m/new`,
              state: {
                title: `Change site name`,
                type: "site-name-change",
                siteId,
                parentBreadcrumbs: [{ path: `/${siteId}/m/`, title: t("common:proposals") }],
                redirectAfterProposalCreation: `/${siteId}/m/`,
                redirectAfterProposalExecution: location.pathname,
                invalidateQueryKeys: sitesKeys.detail(siteId),
              },
            },
          ]
        : []),
      ...(isModeratorVoting("site-text-change", policies)
        ? [
            {
              label: t("textChange"),
              to: `/${siteId}/m/new`,
              state: {
                title: `Change site text`,
                type: "site-text-change",
                siteId,
                parentBreadcrumbs: [{ path: `/${siteId}/m/`, title: t("common:proposals") }],
                redirectAfterProposalCreation: `/${siteId}/m/`,
                redirectAfterProposalExecution: location.pathname,
                invalidateQueryKeys: sitesKeys.detail(siteId),
              },
            },
          ]
        : []),
    ],
    [location.pathname, policies, siteId, t],
  )

  const menuItems = useMemo(
    () => [
      { label: t("common:proposals"), to: `/${siteId}/m` },
      { label: t("common:moderators"), to: `/${siteId}/m/m` },
      { label: t("common:publications"), to: `/${siteId}/m/c` },
      { label: t("common:publishers"), to: `/${siteId}/m/a` },
      { label: t("common:reviews"), to: `/${siteId}/m/r` },
      { label: t("common:users"), to: `/${siteId}/m/u` },
      ...(siteItems.length > 0 ? [{ separator: true }, ...siteItems] : []),
    ],
    [siteId, siteItems, t],
  )

  return menuItems
}
