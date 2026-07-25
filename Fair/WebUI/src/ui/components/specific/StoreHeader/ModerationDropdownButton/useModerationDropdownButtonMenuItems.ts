import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useLocation } from "react-router-dom"

import { useStorePoliciesContext } from "app"
import { storesKeys } from "entities"
import { isModeratorVoting, routes } from "utils"
import { SimpleMenuItem } from "ui/components/SimpleMenu"

export const useModerationDropdownButtonMenuItems = (storeId: string): SimpleMenuItem[] => {
  const location = useLocation()
  const { policies } = useStorePoliciesContext()
  const { t } = useTranslation("storeDropdownMenu")

  const storeItems = useMemo(
    () => [
      ...(isModeratorVoting("store-avatar-change", policies)
        ? [
            {
              label: t("avatarChange"),
              to: routes.moderation.createProposal(storeId),
              state: {
                title: `Change store avatar`,
                type: "store-avatar-change",
                storeId: storeId,
                parentBreadcrumbs: [{ path: routes.moderation.proposals(storeId), title: t("common:proposals") }],
                redirectAfterProposalCreation: routes.moderation.proposals(storeId),
                redirectAfterProposalExecution: location.pathname,
                invalidateQueryKeys: storesKeys.detail(storeId),
              },
            },
          ]
        : []),
      ...(isModeratorVoting("store-name-change", policies)
        ? [
            {
              label: t("nameChange"),
              to: routes.moderation.createProposal(storeId),
              state: {
                title: `Change store name`,
                type: "store-name-change",
                storeId: storeId,
                parentBreadcrumbs: [{ path: routes.moderation.proposals(storeId), title: t("common:proposals") }],
                redirectAfterProposalCreation: routes.moderation.proposals(storeId),
                redirectAfterProposalExecution: location.pathname,
                invalidateQueryKeys: storesKeys.detail(storeId),
              },
            },
          ]
        : []),
      ...(isModeratorVoting("store-info-updation", policies)
        ? [
            {
              label: t("textChange"),
              to: routes.moderation.createProposal(storeId),
              state: {
                title: `Update store information`,
                type: "store-info-updation",
                storeId: storeId,
                parentBreadcrumbs: [{ path: routes.moderation.proposals(storeId), title: t("common:proposals") }],
                redirectAfterProposalCreation: routes.moderation.proposals(storeId),
                redirectAfterProposalExecution: location.pathname,
                invalidateQueryKeys: storesKeys.detail(storeId),
              },
            },
          ]
        : []),
    ],
    [location.pathname, policies, storeId, t],
  )

  const menuItems = useMemo(
    () => [
      { label: t("common:proposals"), to: routes.moderation.proposals(storeId) },
      { label: t("common:moderators"), to: routes.moderation.moderators(storeId) },
      { label: t("common:publications"), to: routes.moderation.publications(storeId) },
      { label: t("common:publishers"), to: routes.moderation.publishers(storeId) },
      { label: t("common:reviews"), to: routes.moderation.reviews(storeId) },
      { label: t("common:users"), to: routes.moderation.users(storeId) },
      ...(storeItems.length > 0 ? [{ separator: true }, ...storeItems] : []),
    ],
    [storeId, storeItems, t],
  )

  return menuItems
}
