import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"
import { truncate } from "lodash"

import { sitesKeys } from "entities"

export const useModeratorUserMenuItems = (userId: string, userName?: string) => {
  const { siteId } = useParams()
  const { t } = useTranslation("moderatorUserMenu")

  const menuItems = useMemo(
    () => [
      {
        label: t("unregisterUser"),
        to: `/${siteId}/m/new`,
        state: {
          title: `Unregister user "${truncate(`${userName} (${userId})`, { length: 46 })}"`,
          type: "user-unregistration",
          userId,
          parentBreadcrumbs: [
            { path: `/${siteId}/m/`, title: t("common:proposals") },
            { path: `/${siteId}/m/u/r/`, title: t("common:users") },
          ],
          redirectAfterProposalCreation: `/${siteId}/m/u/r`,
          redirectAfterProposalExecution: `/${siteId}`,
          invalidateQueryKeys: sitesKeys.users(siteId!),
        },
      },
    ],
    [siteId, t, userId, userName],
  )

  return { menuItems }
}
