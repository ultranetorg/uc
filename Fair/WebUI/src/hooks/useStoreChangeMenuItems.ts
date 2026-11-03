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
  tabKey: string
}

const operations: StoreChangeOperation[] = [
  { type: "store-avatar-change", label: "avatarChange", title: "Change store avatar", tabKey: "avatar-changes" },
  { type: "store-renaming", label: "nameChange", title: "Rename store", tabKey: "renamings" },
  { type: "store-info-updation", label: "textChange", title: "Update store information", tabKey: "info-updations" },
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
  const policies = useStorePoliciesContext()
  const { t } = useTranslation("storeDropdownMenu")

  return useMemo(
    () =>
      operations.flatMap(({ type, label, title, tabKey }): SimpleMenuItem[] => {
        if (isModerator && isModeratorCanCreate(type, policies)) {
          return [
            {
              label: t(label),
              to: routes.moderation.createProposal(storeId),
              state: {
                title,
                type,
                storeId,
                parentBreadcrumbs: [{ path: routes.moderation.store(storeId), title: t("common:store") }],
                redirectAfterProposalCreation: routes.moderation.store(storeId, tabKey),
                redirectAfterProposalExecution: location.pathname,
              },
            },
          ]
        }

        if (isPublisher && isPublisherCanCreate(type, policies)) {
          return [
            {
              label: t(label),
              to: routes.moderation.createProposal(storeId),
              state: {
                title,
                type,
                storeId,
                parentBreadcrumbs: [{ path: routes.moderation.store(storeId), title: t("common:store") }],
                redirectAfterProposalCreation: routes.moderation.store(storeId, tabKey),
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
