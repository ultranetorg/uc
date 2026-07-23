import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { truncate } from "lodash"

import { storesKeys } from "entities"
import { useResolveStoreId } from "hooks"
import { routes } from "utils"

export const useModeratorUserMenuItems = (userId: string, userName?: string) => {
  const storeId = useResolveStoreId()
  const { t } = useTranslation("moderatorUserMenu")

  const menuItems = useMemo(
    () => [
      {
        label: t("unregisterUser"),
        to: routes.moderation.createProposal(storeId!),
        state: {
          title: `Unregister user "${truncate(`${userName} (${userId})`, { length: 46 })}"`,
          type: "user-unregistration",
          userId,
          parentBreadcrumbs: [
            { path: routes.moderation.proposals(storeId!), title: t("common:proposals") },
            { path: routes.moderation.users(storeId!, "remove"), title: t("common:users") },
          ],
          redirectAfterProposalCreation: routes.moderation.users(storeId!, "remove"),
          redirectAfterProposalExecution: routes.store(storeId!),
          invalidateQueryKeys: storesKeys.users(storeId!),
        },
      },
    ],
    [storeId, t, userId, userName],
  )

  return { menuItems }
}
