import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useLocation } from "react-router-dom"

import { useSitePoliciesContext } from "app"
import { sitesKeys } from "entities"
import { isModeratorVoting, routes } from "utils"
import { SimpleMenuItem } from "ui/components/SimpleMenu"

export const useModerationDropdownButtonMenuItems = (siteId: string): SimpleMenuItem[] => {
  const location = useLocation()
  const { policies } = useSitePoliciesContext()
  const { t } = useTranslation("storeDropdownMenu")

  const siteItems = useMemo(
    () => [
      ...(isModeratorVoting("site-avatar-change", policies)
        ? [
            {
              label: t("avatarChange"),
              to: routes.moderation.createProposal(siteId),
              state: {
                title: `Change site avatar`,
                type: "site-avatar-change",
                siteId,
                parentBreadcrumbs: [{ path: routes.moderation.proposals(siteId), title: t("common:proposals") }],
                redirectAfterProposalCreation: routes.moderation.proposals(siteId),
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
              to: routes.moderation.createProposal(siteId),
              state: {
                title: `Change site name`,
                type: "site-name-change",
                siteId,
                parentBreadcrumbs: [{ path: routes.moderation.proposals(siteId), title: t("common:proposals") }],
                redirectAfterProposalCreation: routes.moderation.proposals(siteId),
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
              to: routes.moderation.createProposal(siteId),
              state: {
                title: `Change site text`,
                type: "site-text-change",
                siteId,
                parentBreadcrumbs: [{ path: routes.moderation.proposals(siteId), title: t("common:proposals") }],
                redirectAfterProposalCreation: routes.moderation.proposals(siteId),
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
      { label: t("common:proposals"), to: routes.moderation.proposals(siteId) },
      { label: t("common:moderators"), to: routes.moderation.moderators(siteId) },
      { label: t("common:publications"), to: routes.moderation.publications(siteId) },
      { label: t("common:publishers"), to: routes.moderation.publishers(siteId) },
      { label: t("common:reviews"), to: routes.moderation.reviews(siteId) },
      { label: t("common:users"), to: routes.moderation.users(siteId) },
      ...(siteItems.length > 0 ? [{ separator: true }, ...siteItems] : []),
    ],
    [siteId, siteItems, t],
  )

  return menuItems
}
