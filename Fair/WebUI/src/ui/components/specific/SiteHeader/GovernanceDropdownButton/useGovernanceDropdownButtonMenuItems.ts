import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useLocation } from "react-router-dom"

import { useSitePoliciesContext } from "app"
import { sitesKeys } from "entities"
import { isPublisherVoting, routes } from "utils"
import { SimpleMenuItem } from "ui/components/SimpleMenu"

export const useGovernanceDropdownButtonMenuItems = (siteId: string): SimpleMenuItem[] => {
  const location = useLocation()
  const { policies } = useSitePoliciesContext()
  const { t } = useTranslation("storeDropdownMenu")

  const siteItems = useMemo(
    () => [
      ...(isPublisherVoting("site-avatar-change", policies)
        ? [
            {
              label: t("avatarChange"),
              to: routes.moderation.createProposal(siteId),
              state: {
                title: `Change site avatar`,
                type: "site-avatar-change",
                siteId,
                parentBreadcrumbs: [{ path: routes.moderation.proposals(siteId), title: t("common:referendums") }],
                redirectAfterProposalCreation: routes.moderation.proposals(siteId),
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
              to: routes.governance.createReferendum(siteId),
              state: {
                title: `Change site name`,
                type: "site-name-change",
                siteId,
                parentBreadcrumbs: [{ path: routes.governance.referendums(siteId), title: t("common:referendums") }],
                redirectAfterProposalCreation: routes.governance.referendums(siteId),
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
              to: routes.governance.createReferendum(siteId),
              state: {
                title: `Change site text`,
                type: "site-text-change",
                siteId,
                parentBreadcrumbs: [{ path: routes.governance.referendums(siteId), title: t("common:referendums") }],
                redirectAfterProposalCreation: routes.governance.referendums(siteId),
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
      { label: t("common:surveys"), to: routes.governance.surveys(siteId) },
      { label: t("common:referendums"), to: routes.governance.referendums(siteId) },
      ...(siteItems.length > 0 ? [{ separator: true }, ...siteItems] : []),
    ],
    [siteId, siteItems, t],
  )

  return menuItems
}
