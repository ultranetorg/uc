import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useLocation } from "react-router-dom"
import { truncate } from "lodash"

import { useParams, useResolveStoreId } from "hooks"
import { routes } from "utils"

export const useCategoryContextThreeDotsMenu = (categoryId: string, categoryTitle: string) => {
  const location = useLocation()
  const { t } = useTranslation("moderatorCategoryMenu")
  const { categoryId: paramCategoryId } = useParams()
  const storeId = useResolveStoreId()

  const menuItems = useMemo(
    () => [
      {
        label: t("moderatorCategoryMenu:avatarChange"),
        to: routes.moderation.createProposal(storeId!),
        state: {
          title: truncate(`Change category "${categoryTitle}" avatar`, { length: 64 }),
          type: "category-avatar-change",
          categoryId,
          parentBreadcrumbs: [{ path: routes.moderation.proposals(storeId!), title: t("common:proposals") }],
          redirectAfterProposalCreation: routes.moderation.proposals(storeId!),
          redirectAfterProposalExecution: location.pathname,
        },
      },
      {
        label: t("moderatorCategoryMenu:typeChange"),
        to: routes.moderation.createProposal(storeId!),
        state: {
          title: truncate(`Change category "${categoryTitle}" type`, { length: 64 }),
          type: "category-type-change",
          categoryId,
          parentBreadcrumbs: [{ path: routes.moderation.proposals(storeId!), title: t("common:proposals") }],
          redirectAfterProposalCreation: routes.moderation.proposals(storeId!),
          redirectAfterProposalExecution: location.pathname,
        },
      },
      {
        label: t("moderatorCategoryMenu:move"),
        to: routes.moderation.createProposal(storeId!),
        state: {
          title: truncate(`Move "${categoryTitle}" category`, { length: 64 }),
          type: "category-movement",
          categoryId,
          parentBreadcrumbs: [
            { path: routes.moderation.proposals(storeId!), title: t("common:proposals") },
            { path: routes.moderation.publications(storeId!), title: t("common:publications") },
          ],
          redirectAfterProposalCreation: routes.moderation.proposals(storeId!),
          redirectAfterProposalExecution: paramCategoryId === categoryId ? routes.store(storeId!) : location.pathname,
        },
      },
      {
        label: t("moderatorCategoryMenu:categoryCreate"),
        to: routes.moderation.createProposal(storeId!),
        state: {
          title: "Create new category",
          type: "category-creation",
          categoryId,
          parentBreadcrumbs: [
            { path: routes.moderation.proposals(storeId!), title: t("common:proposals") },
            { path: routes.moderation.publications(storeId!), title: t("common:publications") },
          ],
          redirectAfterProposalCreation: routes.moderation.proposals(storeId!),
          redirectAfterProposalExecution: location.pathname,
        },
      },
      {
        separator: true,
      },
      {
        label: t("moderatorCategoryMenu:remove"),
        to: routes.moderation.createProposal(storeId!),
        state: {
          title: truncate(`Remove category "${categoryTitle}"`, { length: 64 }),
          type: "category-deletion",
          categoryId,
          parentBreadcrumbs: [{ path: routes.moderation.proposals(storeId!), title: t("common:proposals") }],
          redirectAfterProposalCreation: routes.moderation.proposals(storeId!),
          redirectAfterProposalExecution: paramCategoryId === categoryId ? routes.store(storeId!) : location.pathname,
        },
      },
    ],
    [t, storeId, categoryTitle, categoryId, location.pathname, paramCategoryId],
  )

  return menuItems
}
