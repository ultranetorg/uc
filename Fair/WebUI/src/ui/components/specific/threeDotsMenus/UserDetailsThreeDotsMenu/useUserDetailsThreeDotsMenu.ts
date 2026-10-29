import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { truncate } from "lodash"

import { useCanCreateOperation, useResolveStoreId } from "hooks"
import { routes } from "utils"

export const useUserDetailsThreeDotsMenu = (userId: string, userName?: string) => {
  const { t } = useTranslation("moderatorUserMenu")

  const canCreateOperation = useCanCreateOperation()
  const storeId = useResolveStoreId()

  const isUnregistrationVisible = canCreateOperation("user-unregistration")

  return useMemo(
    () =>
      isUnregistrationVisible
        ? [
            {
              label: t("unregisterUser"),
              to: routes.moderation.createProposal(storeId!),
              state: {
                title: truncate(`Unregister user "${userName} (${userId})"`, { length: 64 }),
                type: "user-unregistration",
                userId,
                parentBreadcrumbs: [
                  { path: routes.moderation.proposals(storeId!), title: t("common:proposals") },
                  { path: routes.moderation.users(storeId!, "remove"), title: t("common:users") },
                ],
                redirectAfterProposalCreation: routes.moderation.users(storeId!, "remove"),
                redirectAfterProposalExecution: routes.store(storeId!),
              },
            },
          ]
        : [],
    [isUnregistrationVisible, storeId, t, userId, userName],
  )
}
