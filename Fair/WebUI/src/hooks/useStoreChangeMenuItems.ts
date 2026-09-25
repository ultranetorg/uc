import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useLocation } from "react-router-dom"

import { useStorePoliciesContext } from "app"
import { OperationType } from "types"
import { SimpleMenuItem } from "ui/components"
import { isModeratorVoting, isPublisherVoting, routes } from "utils"

type StoreChangeOperation = {
  type: OperationType
  label: string
  title: string
}

const operations: StoreChangeOperation[] = [
  { type: "store-avatar-change", label: "avatarChange", title: "Change store avatar" },
  { type: "store-renaming", label: "nameChange", title: "Rename store" },
  { type: "store-info-updation", label: "textChange", title: "Update store information" },
]

export type UseStoreChangeMenuItemsOptions = {
  moderators?: boolean
  publishers?: boolean
}

export const useStoreChangeMenuItems = (
  storeId: string,
  { moderators = true, publishers = true }: UseStoreChangeMenuItemsOptions = {},
): SimpleMenuItem[] => {
  const location = useLocation()
  const { policies } = useStorePoliciesContext()
  const { t } = useTranslation("storeDropdownMenu")

  return useMemo(
    () =>
      operations.flatMap(({ type, label, title }): SimpleMenuItem[] => {
        if (moderators && isModeratorVoting(type, policies)) {
          return [
            {
              label: t(label),
              to: routes.moderation.createProposal(storeId),
              state: {
                title,
                type,
                storeId,
                parentBreadcrumbs: [{ path: routes.moderation.proposals(storeId), title: t("common:proposals") }],
                redirectAfterProposalCreation: routes.moderation.proposals(storeId),
                redirectAfterProposalExecution: location.pathname,
              },
            },
          ]
        }

        if (publishers && isPublisherVoting(type, policies)) {
          return [
            {
              label: t(label),
              to: routes.governance.createReferendum(storeId),
              state: {
                title,
                type,
                storeId,
                parentBreadcrumbs: [{ path: routes.governance.referendums(storeId), title: t("common:referendums") }],
                redirectAfterProposalCreation: routes.governance.referendums(storeId),
                redirectAfterProposalExecution: location.pathname,
              },
            },
          ]
        }

        return []
      }),
    [location.pathname, moderators, policies, publishers, storeId, t],
  )
}
