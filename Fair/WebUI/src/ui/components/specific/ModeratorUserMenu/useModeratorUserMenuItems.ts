import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { truncate } from "lodash"

import { sitesKeys } from "entities"
import { useParams } from "hooks"
import { routes } from "utils"

export const useModeratorUserMenuItems = (userId: string, userName?: string) => {
  const { siteId } = useParams()
  const { t } = useTranslation("moderatorUserMenu")

  const menuItems = useMemo(
    () => [
      {
        label: t("unregisterUser"),
        to: routes.moderation.create(siteId!),
        state: {
          title: `Unregister user "${truncate(`${userName} (${userId})`, { length: 46 })}"`,
          type: "user-unregistration",
          userId,
          parentBreadcrumbs: [
            { path: routes.moderation.root(siteId!), title: t("common:proposals") },
            { path: routes.moderation.users(siteId!, "r"), title: t("common:users") },
          ],
          redirectAfterProposalCreation: routes.moderation.users(siteId!, "r"),
          redirectAfterProposalExecution: routes.site(siteId!),
          invalidateQueryKeys: sitesKeys.users(siteId!),
        },
      },
    ],
    [siteId, t, userId, userName],
  )

  return { menuItems }
}
