import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { truncate } from "lodash"

import { useIsOperationVisibleForCreator, useResolveStoreId } from "hooks"
import { routes } from "utils"

export const useUserDetailsThreeDotsMenu = (userId: string, userName?: string) => {
  const storeId = useResolveStoreId()
  const { t } = useTranslation("moderatorUserMenu")
  const isOperationVisibleForCreator = useIsOperationVisibleForCreator()

  const isUnregistrationVisible = isOperationVisibleForCreator("user-unregistration")

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
