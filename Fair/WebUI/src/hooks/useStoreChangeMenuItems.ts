import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useLocation } from "react-router-dom"

import { useStorePoliciesContext } from "app"
import { OperationType } from "types"
import { SimpleMenuItem } from "ui/components"
import { isModeratorCanCreate, isPublisherCanCreate, routes } from "utils"

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
  isModerator?: boolean
  isPublisher?: boolean
}

export const useStoreChangeMenuItems = (
  storeId: string,
  { isModerator, isPublisher }: UseStoreChangeMenuItemsOptions = {},
): SimpleMenuItem[] => {
  const location = useLocation()
  const { policies } = useStorePoliciesContext()
  const { t } = useTranslation("storeDropdownMenu")

  return useMemo(
    () =>
      operations.flatMap(({ type, label, title }): SimpleMenuItem[] => {
        if (isModerator && isModeratorCanCreate(type, policies)) {
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

        if (isPublisher && isPublisherCanCreate(type, policies)) {
          return [
            {
              label: t(label),
              to: routes.moderation.createReferendum(storeId),
              state: {
                title,
                type,
                storeId,
                parentBreadcrumbs: [{ path: routes.moderation.referendums(storeId), title: t("common:referendums") }],
                redirectAfterProposalCreation: routes.moderation.referendums(storeId),
                redirectAfterProposalExecution: location.pathname,
              },
            },
          ]
        }

        return []
      }),
    [location.pathname, isModerator, policies, isPublisher, storeId, t],
  )
}
