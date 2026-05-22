import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useLocation } from "react-router-dom"

import { useModerationContext } from "app"
import { sitesKeys } from "entities"
import { isPublisherVoting } from "utils"
import { SimpleMenuItem } from "ui/components/SimpleMenu"

export const useGovernanceDropdownButtonMenuItems = (siteId: string): SimpleMenuItem[] => {
  const location = useLocation()
  const { policies } = useModerationContext()
  const { t } = useTranslation("storeDropdownMenu")

  const siteItems = useMemo(
    () => [
      ...(isPublisherVoting("site-avatar-change", policies)
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
      ...(isPublisherVoting("site-name-change", policies)
        ? [
            {
              label: t("nameChange"),
              to: `/${siteId}/g/new`,
              state: {
                title: `Change site name`,
                type: "site-name-change",
                siteId,
                parentBreadcrumbs: [{ path: `/${siteId}/g/r/`, title: t("common:proposals") }],
                redirectAfterProposalCreation: `/${siteId}/g/r/`,
                redirectAfterProposalExecution: location.pathname,
                invalidateQueryKeys: sitesKeys.detail(siteId),
              },
            },
          ]
        : []),
      ...(isPublisherVoting("site-text-change", policies)
        ? [
            {
              label: t("textChange"),
              to: `/${siteId}/g/new`,
              state: {
                title: `Change site text`,
                type: "site-text-change",
                siteId,
                parentBreadcrumbs: [{ path: `/${siteId}/g/`, title: t("common:proposals") }],
                redirectAfterProposalCreation: `/${siteId}/g/`,
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
      { label: t("common:surveys"), to: `/${siteId}/g/p` },
      { label: t("common:referendums"), to: `/${siteId}/g/r` },
      ...(siteItems.length > 0 ? [{ separator: true }, ...siteItems] : []),
    ],
    [siteId, siteItems, t],
  )

  return menuItems
}
