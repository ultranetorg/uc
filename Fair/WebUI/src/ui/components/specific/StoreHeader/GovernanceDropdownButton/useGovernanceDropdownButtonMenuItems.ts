import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useLocation } from "react-router-dom"

import { useStorePoliciesContext } from "app"
import { storesKeys } from "entities"
import { isPublisherVoting, routes } from "utils"
import { SimpleMenuItem } from "ui/components/SimpleMenu"

export const useGovernanceDropdownButtonMenuItems = (storeId: string): SimpleMenuItem[] => {
  const location = useLocation()
  const { policies } = useStorePoliciesContext()
  const { t } = useTranslation("storeDropdownMenu")

  const storeItems = useMemo(
    () => [
      ...(isPublisherVoting("store-avatar-change", policies)
        ? [
            {
              label: t("avatarChange"),
              to: routes.moderation.createProposal(storeId),
              state: {
                title: `Change store avatar`,
                type: "store-avatar-change",
                storeId: storeId,
                parentBreadcrumbs: [{ path: routes.moderation.proposals(storeId), title: t("common:referendums") }],
                redirectAfterProposalCreation: routes.moderation.proposals(storeId),
                redirectAfterProposalExecution: location.pathname,
                invalidateQueryKeys: storesKeys.detail(storeId),
              },
            },
          ]
        : []),
      ...(isPublisherVoting("store-name-change", policies)
        ? [
            {
              label: t("nameChange"),
              to: routes.governance.createReferendum(storeId),
              state: {
                title: `Change store name`,
                type: "store-name-change",
                storeId: storeId,
                parentBreadcrumbs: [{ path: routes.governance.referendums(storeId), title: t("common:referendums") }],
                redirectAfterProposalCreation: routes.governance.referendums(storeId),
                redirectAfterProposalExecution: location.pathname,
                invalidateQueryKeys: storesKeys.detail(storeId),
              },
            },
          ]
        : []),
      ...(isPublisherVoting("store-info-updation", policies)
        ? [
            {
              label: t("textChange"),
              to: routes.governance.createReferendum(storeId),
              state: {
                title: `Update store information`,
                type: "store-info-updation",
                storeId: storeId,
                parentBreadcrumbs: [{ path: routes.governance.referendums(storeId), title: t("common:referendums") }],
                redirectAfterProposalCreation: routes.governance.referendums(storeId),
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
      { label: t("common:surveys"), to: routes.governance.surveys(storeId) },
      { label: t("common:referendums"), to: routes.governance.referendums(storeId) },
      ...(storeItems.length > 0 ? [{ separator: true }, ...storeItems] : []),
    ],
    [storeId, storeItems, t],
  )

  return menuItems
}
